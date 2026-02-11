# Performance Guide

This document covers performance characteristics and optimization strategies for Tombatron.Turbo.

## Overview

Tombatron.Turbo is designed for high performance with minimal overhead. Key performance features:

- Compile-time frame validation (no runtime parsing)
- Efficient HTML generation with string builders
- SignalR's optimized WebSocket connections
- Reference counting for shared connections

## Benchmarks

Run the benchmarks yourself:

```bash
cd tests/Tombatron.Turbo.Benchmarks
dotnet run -c Release
```

### Stream Builder Performance

Typical performance for stream building operations:

| Operation | Time | Memory |
|-----------|------|--------|
| Single action (small HTML) | ~200ns | ~500 bytes |
| Single action (large HTML) | ~500ns | ~2KB |
| 10 actions | ~1μs | ~3KB |
| 50 actions | ~5μs | ~15KB |

### Frame Parser Performance

Compile-time parsing performance (during build):

| Document Size | Frames | Time |
|---------------|--------|------|
| Small (1KB) | 1 | ~50μs |
| Medium (5KB) | 5 | ~200μs |
| Large (50KB) | 50 | ~2ms |

## Optimization Strategies

### 1. Batch Stream Actions

Combine multiple updates into a single stream message:

```csharp
// Good - single message
await turbo.Stream("dashboard", builder =>
{
    builder
        .Update("count", "42")
        .Update("status", "Online")
        .Append("log", "<li>Updated</li>");
});

// Less efficient - multiple messages
await turbo.Stream("dashboard", b => b.Update("count", "42"));
await turbo.Stream("dashboard", b => b.Update("status", "Online"));
await turbo.Stream("dashboard", b => b.Append("log", "<li>Updated</li>"));
```

### 2. Use Targeted Streams

Send updates only to clients who need them:

```csharp
// Good - only affected user receives update
await turbo.Stream($"user:{userId}", builder =>
{
    builder.Update("notifications", notificationHtml);
});

// Less efficient - all clients receive update
await turbo.Broadcast(builder =>
{
    builder.Update("notifications", notificationHtml);
});
```

### 3. Minimize HTML Size

Send only the necessary HTML:

```csharp
// Good - minimal update
builder.Update("cart-count", "5");

// Less efficient - full component
builder.Replace("cart-header", @"
    <div class='cart-header'>
        <span class='icon'>🛒</span>
        <span class='count'>5</span>
        <span class='label'>items</span>
    </div>
");
```

### 4. Use Appropriate Action Types

Choose the right action for your update:

| Action | Use When |
|--------|----------|
| `Update` | Replacing inner content only |
| `Replace` | Replacing entire element |
| `Append` | Adding to a list |
| `Remove` | Deleting elements |

`Update` is more efficient than `Replace` when you only need to change content.

### 5. Lazy Loading with Frames

Use lazy loading for heavy content:

```html
<turbo-frame id="heavy-content" src="/api/heavy-content" loading="lazy">
    <div class="loading">Loading...</div>
</turbo-frame>
```

### 6. Connection Management

The client-side JavaScript uses a singleton SignalR connection with reference counting:

- Multiple `<turbo-stream-source-signalr>` elements share one connection
- Connection is closed only when all elements are removed
- Automatic reconnection on disconnect

```html
<!-- These share a single connection -->
<turbo-stream-source-signalr stream="user:123"></turbo-stream-source-signalr>
<turbo-stream-source-signalr stream="notifications"></turbo-stream-source-signalr>
```

## Scaling Considerations

### Horizontal Scaling

For multi-server deployments, use a SignalR backplane:

```csharp
// Redis backplane
services.AddSignalR()
    .AddStackExchangeRedis("localhost:6379");

// Azure SignalR Service
services.AddSignalR()
    .AddAzureSignalR();
```

### Connection Limits

Default SignalR limits:
- Max connections per server: ~10,000 (depends on hardware)
- Message size: 32KB default

Configure as needed:

```csharp
services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
});
```

### Memory Usage

Tips for reducing memory:
- Reuse HTML templates
- Use string interpolation efficiently
- Avoid creating large intermediate strings
- Consider pagination for large datasets

## Monitoring

### Logging

Enable debug logging for performance analysis:

```json
{
  "Logging": {
    "LogLevel": {
      "Tombatron.Turbo": "Debug"
    }
  }
}
```

### Metrics

Consider adding custom metrics:

```csharp
public class InstrumentedTurboService : ITurbo
{
    private readonly ITurbo _inner;
    private readonly IMeterFactory _meterFactory;
    private readonly Counter<long> _streamCount;

    public InstrumentedTurboService(ITurbo inner, IMeterFactory meterFactory)
    {
        _inner = inner;
        _meterFactory = meterFactory;
        var meter = _meterFactory.Create("Tombatron.Turbo");
        _streamCount = meter.CreateCounter<long>("turbo.streams.sent");
    }

    public async Task Stream(string streamName, Action<ITurboStreamBuilder> build)
    {
        _streamCount.Add(1, new("stream", streamName));
        await _inner.Stream(streamName, build);
    }

    // ... other methods
}
```

### Health Checks

Add a health check for SignalR:

```csharp
services.AddHealthChecks()
    .AddCheck<SignalRHealthCheck>("signalr");

public class SignalRHealthCheck : IHealthCheck
{
    private readonly IHubContext<TurboHub> _hubContext;

    public SignalRHealthCheck(IHubContext<TurboHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // SignalR hub context is available
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
```

## Best Practices Summary

1. **Batch updates** - Combine multiple actions into single stream messages
2. **Target precisely** - Send updates only to affected clients
3. **Minimize payload** - Send only necessary HTML
4. **Use lazy loading** - Defer heavy content with frame lazy loading
5. **Monitor performance** - Enable logging and metrics
6. **Scale with backplanes** - Use Redis or Azure SignalR for multi-server
7. **Test under load** - Benchmark with realistic concurrent connections
