namespace Tombatron.Turbo.Dashboard;

/// <summary>
/// Service that holds current dashboard metrics.
/// In a real application, these would come from a monitoring system.
/// </summary>
public class MetricsService
{
    private readonly Random _random = new();
    private readonly object _lock = new();

    public int ActiveUsers { get; private set; } = 1247;
    public int RequestsPerSecond { get; private set; } = 3421;
    public double CpuUsage { get; private set; } = 42.5;
    public double MemoryUsage { get; private set; } = 68.3;
    public int ErrorCount { get; private set; } = 12;
    public double ResponseTime { get; private set; } = 45.2;
    public List<ActivityItem> RecentActivity { get; } = new();
    public List<double> ResponseTimeHistory { get; } = new();

    public MetricsService()
    {
        // Initialize with some sample data
        for (int i = 0; i < 20; i++)
        {
            ResponseTimeHistory.Add(30 + _random.NextDouble() * 40);
        }

        RecentActivity.Add(new ActivityItem("User signup", "New user 'alice' registered", DateTime.UtcNow.AddMinutes(-5)));
        RecentActivity.Add(new ActivityItem("Order placed", "Order #12345 for $99.00", DateTime.UtcNow.AddMinutes(-3)));
        RecentActivity.Add(new ActivityItem("Payment received", "Payment of $149.00 processed", DateTime.UtcNow.AddMinutes(-1)));
    }

    public void UpdateMetrics()
    {
        lock (_lock)
        {
            // Simulate realistic metric changes
            ActiveUsers = Math.Max(0, ActiveUsers + _random.Next(-50, 60));
            RequestsPerSecond = Math.Max(0, RequestsPerSecond + _random.Next(-200, 250));
            CpuUsage = Math.Clamp(CpuUsage + (_random.NextDouble() - 0.5) * 10, 5, 95);
            MemoryUsage = Math.Clamp(MemoryUsage + (_random.NextDouble() - 0.5) * 5, 20, 95);
            ErrorCount = Math.Max(0, ErrorCount + _random.Next(-2, 4));
            ResponseTime = Math.Clamp(ResponseTime + (_random.NextDouble() - 0.5) * 20, 10, 200);

            // Update response time history
            ResponseTimeHistory.Add(ResponseTime);
            if (ResponseTimeHistory.Count > 20)
            {
                ResponseTimeHistory.RemoveAt(0);
            }
        }
    }

    public void AddActivity(string type, string description)
    {
        lock (_lock)
        {
            RecentActivity.Insert(0, new ActivityItem(type, description, DateTime.UtcNow));
            if (RecentActivity.Count > 10)
            {
                RecentActivity.RemoveAt(RecentActivity.Count - 1);
            }
        }
    }

    public string GetCpuStatus() => CpuUsage switch
    {
        < 50 => "healthy",
        < 80 => "warning",
        _ => "critical"
    };

    public string GetMemoryStatus() => MemoryUsage switch
    {
        < 60 => "healthy",
        < 85 => "warning",
        _ => "critical"
    };

    public string GetResponseTimeStatus() => ResponseTime switch
    {
        < 50 => "healthy",
        < 100 => "warning",
        _ => "critical"
    };
}

public class ActivityItem
{
    public string Type { get; }
    public string Description { get; }
    public DateTime Timestamp { get; }

    public ActivityItem(string type, string description, DateTime timestamp)
    {
        Type = type;
        Description = description;
        Timestamp = timestamp;
    }
}

/// <summary>
/// Background service that periodically updates metrics and broadcasts to all clients.
/// </summary>
public class MetricsUpdater : BackgroundService
{
    private readonly MetricsService _metrics;
    private readonly ITurbo _turbo;
    private readonly ILogger<MetricsUpdater> _logger;
    private readonly Random _random = new();

    private readonly string[] _activityTypes = { "User signup", "Order placed", "Payment received", "Product viewed", "Cart updated" };
    private readonly string[] _userNames = { "alice", "bob", "charlie", "diana", "eve", "frank" };

    public MetricsUpdater(MetricsService metrics, ITurbo turbo, ILogger<MetricsUpdater> logger)
    {
        _metrics = metrics;
        _turbo = turbo;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics updater started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Update metrics
                _metrics.UpdateMetrics();

                // Occasionally add a new activity
                if (_random.Next(100) < 30) // 30% chance each update
                {
                    var type = _activityTypes[_random.Next(_activityTypes.Length)];
                    var description = type switch
                    {
                        "User signup" => $"New user '{_userNames[_random.Next(_userNames.Length)]}' registered",
                        "Order placed" => $"Order #{_random.Next(10000, 99999)} for ${_random.Next(10, 500)}.00",
                        "Payment received" => $"Payment of ${_random.Next(10, 500)}.00 processed",
                        "Product viewed" => $"Product SKU-{_random.Next(1000, 9999)} viewed",
                        "Cart updated" => $"Cart updated with {_random.Next(1, 5)} items",
                        _ => "Activity occurred"
                    };
                    _metrics.AddActivity(type, description);
                }

                // Broadcast updates to all connected clients
                await _turbo.Broadcast(builder =>
                {
                    // Update metric cards
                    builder.Update("metric-users", FormatNumber(_metrics.ActiveUsers));
                    builder.Update("metric-rps", FormatNumber(_metrics.RequestsPerSecond));
                    builder.Update("metric-cpu", $"{_metrics.CpuUsage:F1}%");
                    builder.Update("metric-memory", $"{_metrics.MemoryUsage:F1}%");
                    builder.Update("metric-errors", _metrics.ErrorCount.ToString());
                    builder.Update("metric-response", $"{_metrics.ResponseTime:F1}ms");

                    // Update status indicators
                    builder.Update("status-cpu", RenderStatus(_metrics.GetCpuStatus()));
                    builder.Update("status-memory", RenderStatus(_metrics.GetMemoryStatus()));
                    builder.Update("status-response", RenderStatus(_metrics.GetResponseTimeStatus()));

                    // Update mini chart
                    builder.Update("chart-data", RenderChart(_metrics.ResponseTimeHistory));

                    // Update activity feed
                    builder.Update("activity-feed", RenderActivityFeed(_metrics.RecentActivity));
                });

                await Task.Delay(2000, stoppingToken); // Update every 2 seconds
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metrics");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Metrics updater stopped");
    }

    private static string FormatNumber(int value)
    {
        return value >= 1000 ? $"{value / 1000.0:F1}k" : value.ToString();
    }

    private static string RenderStatus(string status)
    {
        var color = status switch
        {
            "healthy" => "#10b981",
            "warning" => "#f59e0b",
            "critical" => "#ef4444",
            _ => "#6b7280"
        };
        return $"<span style=\"display:inline-block;width:8px;height:8px;border-radius:50%;background:{color}\"></span>";
    }

    private static string RenderChart(List<double> data)
    {
        if (data.Count < 2)
        {
            return "";
        }

        var max = data.Max();
        var min = data.Min();
        var range = max - min;
        if (range < 1)
        {
            range = 1;
        }

        var width = 200;
        var height = 40;
        var stepX = (double)width / (data.Count - 1);

        var points = new List<string>();
        for (int i = 0; i < data.Count; i++)
        {
            var x = i * stepX;
            var y = height - ((data[i] - min) / range * height);
            points.Add($"{x:F1},{y:F1}");
        }

        return $@"<svg width=""{width}"" height=""{height}"" style=""overflow:visible"">
            <polyline fill=""none"" stroke=""#5865f2"" stroke-width=""2"" points=""{string.Join(" ", points)}""/>
        </svg>";
    }

    private static string RenderActivityFeed(List<ActivityItem> items)
    {
        var html = new System.Text.StringBuilder();
        foreach (var item in items.Take(5))
        {
            var timeAgo = GetTimeAgo(item.Timestamp);
            html.AppendLine($@"
<div class=""activity-item"">
    <div class=""activity-icon"">{GetActivityIcon(item.Type)}</div>
    <div class=""activity-content"">
        <div class=""activity-title"">{System.Net.WebUtility.HtmlEncode(item.Description)}</div>
        <div class=""activity-time"">{timeAgo}</div>
    </div>
</div>");
        }

        return html.ToString();
    }

    private static string GetActivityIcon(string type) => type switch
    {
        "User signup" => "👤",
        "Order placed" => "📦",
        "Payment received" => "💳",
        "Product viewed" => "👁",
        "Cart updated" => "🛒",
        _ => "📌"
    };

    private static string GetTimeAgo(DateTime timestamp)
    {
        var diff = DateTime.UtcNow - timestamp;
        if (diff.TotalSeconds < 60) { return "just now"; }

        if (diff.TotalMinutes < 60) { return $"{(int)diff.TotalMinutes}m ago"; }

        if (diff.TotalHours < 24) { return $"{(int)diff.TotalHours}h ago"; }

        return $"{(int)diff.TotalDays}d ago";
    }
}
