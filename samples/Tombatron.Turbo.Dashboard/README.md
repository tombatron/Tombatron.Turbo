# Live Dashboard Sample

A real-time metrics dashboard demonstrating Turbo Streams broadcast functionality. Metrics update automatically every 2 seconds for all connected clients.

## Running the Sample

```bash
cd samples/Tombatron.Turbo.Dashboard
dotnet run
```

Open https://localhost:5001 in your browser. Open multiple tabs to see synchronized updates!

## Features Demonstrated

### 1. Broadcast to All Clients

The dashboard uses `ITurbo.Broadcast()` to send updates to every connected client simultaneously:

```csharp
// Background service broadcasts metrics every 2 seconds
await _turbo.Broadcast(builder =>
{
    builder.Update("metric-users", FormatNumber(_metrics.ActiveUsers));
    builder.Update("metric-rps", FormatNumber(_metrics.RequestsPerSecond));
    builder.Update("metric-cpu", $"{_metrics.CpuUsage:F1}%");
    // ... more updates
});
```

### 2. Multiple Updates in Single Message

A single broadcast can update many DOM elements at once:

```csharp
await _turbo.Broadcast(builder =>
{
    // Update all metric values
    builder.Update("metric-users", "1.2k");
    builder.Update("metric-rps", "3.4k");
    builder.Update("metric-cpu", "42.5%");
    builder.Update("metric-memory", "68.3%");

    // Update status indicators
    builder.Update("status-cpu", RenderStatus("healthy"));
    builder.Update("status-memory", RenderStatus("warning"));

    // Update complex content
    builder.Update("chart-data", RenderChart(data));
    builder.Update("activity-feed", RenderActivityFeed(items));
});
```

### 3. Background Service Integration

A hosted service runs continuously and broadcasts updates:

```csharp
public class MetricsUpdater : BackgroundService
{
    private readonly ITurbo _turbo;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Update metrics
            _metrics.UpdateMetrics();

            // Broadcast to all clients
            await _turbo.Broadcast(builder => { ... });

            await Task.Delay(2000, stoppingToken);
        }
    }
}
```

### 4. SVG Chart Updates

The response time chart is rendered as inline SVG and updated in real-time:

```csharp
private static string RenderChart(List<double> data)
{
    // Calculate points for SVG polyline
    var points = new List<string>();
    for (int i = 0; i < data.Count; i++)
    {
        var x = i * stepX;
        var y = height - ((data[i] - min) / range * height);
        points.Add($"{x:F1},{y:F1}");
    }

    return $@"<svg width=""{width}"" height=""{height}"">
        <polyline stroke=""#5865f2"" points=""{string.Join(" ", points)}""/>
    </svg>";
}
```

### 5. Status Indicators

Visual status indicators change color based on metric thresholds:

```csharp
public string GetCpuStatus() => CpuUsage switch
{
    < 50 => "healthy",   // Green
    < 80 => "warning",   // Yellow
    _ => "critical"      // Red
};

private static string RenderStatus(string status)
{
    var color = status switch
    {
        "healthy" => "#10b981",
        "warning" => "#f59e0b",
        "critical" => "#ef4444",
        _ => "#6b7280"
    };
    return $"<span style=\"background:{color}\" class=\"status-dot\"></span>";
}
```

## Project Structure

```
Tombatron.Turbo.Dashboard/
├── Program.cs                    # App setup with hosted service
├── MetricsService.cs             # Metrics storage + background updater
├── Pages/
│   ├── Index.cshtml              # Dashboard UI
│   ├── Index.cshtml.cs           # Initial render helpers
│   └── Shared/
│       └── _Layout.cshtml        # Layout with dark theme
└── README.md
```

## Key Patterns

### Efficient Updates

Instead of sending full page HTML, only the changed values are transmitted:

```html
<!-- Server sends only the new value -->
<turbo-stream action="update" target="metric-users">
    <template>1.3k</template>
</turbo-stream>
```

### Idempotent Rendering

The same render functions are used for both initial page load and stream updates:

```csharp
// Used in page model for initial render
public string RenderChart() => RenderChartHtml(_metrics.ResponseTimeHistory);

// Same logic used in background service for updates
builder.Update("chart-data", RenderChartHtml(_metrics.ResponseTimeHistory));
```

### Graceful Connection Handling

The connection status indicator shows the current state:

```javascript
document.addEventListener('turbo:signalr:connected', () => {
    updateStatus('connected', 'Live');
});

document.addEventListener('turbo:signalr:reconnecting', () => {
    updateStatus('connecting', 'Reconnecting...');
});
```

## Try It Out

1. Open the dashboard in multiple browser windows
2. Watch metrics update simultaneously across all windows
3. Check the CPU/Memory/Response status indicators
4. See the activity feed update with new events
5. Watch the response time chart animate

## Use Cases

This pattern is ideal for:
- Admin dashboards
- Monitoring systems
- Live scoreboards
- Stock tickers
- IoT sensor displays
- Any scenario requiring push-based updates to all clients

## Learn More

- [Turbo Streams Guide](../../docs/guides/turbo-streams.md)
- [Performance Guide](../../docs/performance.md)
