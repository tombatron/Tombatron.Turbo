using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Tombatron.Turbo.Dashboard.Pages;

public class IndexModel : PageModel
{
    public MetricsService Metrics { get; }

    public IndexModel(MetricsService metrics)
    {
        Metrics = metrics;
    }

    public void OnGet()
    {
    }

    public string FormatNumber(int value)
    {
        return value >= 1000 ? $"{value / 1000.0:F1}k" : value.ToString();
    }

    public string RenderStatus(string status)
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

    public string RenderChart()
    {
        var data = Metrics.ResponseTimeHistory;

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

    public string RenderActivityFeed()
    {
        var html = new System.Text.StringBuilder();
        foreach (var item in Metrics.RecentActivity.Take(5))
        {
            var timeAgo = GetTimeAgo(item.Timestamp);
            var icon = item.Type switch
            {
                "User signup" => "👤",
                "Order placed" => "📦",
                "Payment received" => "💳",
                "Product viewed" => "👁",
                "Cart updated" => "🛒",
                _ => "📌"
            };

            html.AppendLine($@"
<div class=""activity-item"">
    <div class=""activity-icon"">{icon}</div>
    <div class=""activity-content"">
        <div class=""activity-title"">{System.Net.WebUtility.HtmlEncode(item.Description)}</div>
        <div class=""activity-time"">{timeAgo}</div>
    </div>
</div>");
        }

        return html.ToString();
    }

    private static string GetTimeAgo(DateTime timestamp)
    {
        var diff = DateTime.UtcNow - timestamp;
        
        if (diff.TotalSeconds < 60)
        {
            return "just now";
        }

        if (diff.TotalMinutes < 60)
        {
            return $"{(int)diff.TotalMinutes}m ago";
        }

        if (diff.TotalHours < 24)
        {
            return $"{(int)diff.TotalHours}h ago";
        }

        return $"{(int)diff.TotalDays}d ago";
    }
}
