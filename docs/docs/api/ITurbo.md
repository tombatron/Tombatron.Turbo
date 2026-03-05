---
title: ITurbo Interface
sidebar_label: ITurbo
sidebar_position: 1
description: The main service interface for broadcasting Turbo Stream updates to connected clients via SignalR.
---

The main service interface for broadcasting Turbo Stream updates to connected clients via SignalR.

## Namespace

```csharp
Tombatron.Turbo
```

## Usage

Inject `ITurbo` into your controllers, page models, or services:

```csharp
public class CartController : Controller
{
    private readonly ITurbo _turbo;

    public CartController(ITurbo turbo)
    {
        _turbo = turbo;
    }
}
```

## Methods

### Stream(string streamName, Action\<ITurboStreamBuilder\> build)

Broadcasts Turbo Stream updates to all clients subscribed to the specified stream.

**Parameters:**
- `streamName` - The name of the stream to broadcast to
- `build` - An action that configures the stream updates to send

**Returns:** A `Task` that completes when the broadcast has been sent.

**Exceptions:**
- `ArgumentNullException` - When `streamName` or `build` is null
- `ArgumentException` - When `streamName` is empty or whitespace

**Example:**

```csharp
await _turbo.Stream("cart:123", builder =>
{
    builder.Replace("cart-total", "<span>$99.99</span>");
});
```

### Stream(IEnumerable\<string\> streamNames, Action\<ITurboStreamBuilder\> build)

Broadcasts Turbo Stream updates to all clients subscribed to any of the specified streams.

**Parameters:**
- `streamNames` - The names of the streams to broadcast to
- `build` - An action that configures the stream updates to send

**Returns:** A `Task` that completes when all broadcasts have been sent.

**Exceptions:**
- `ArgumentNullException` - When `streamNames` or `build` is null

**Example:**

```csharp
// Notify multiple users
await _turbo.Stream(new[] { "user:alice", "user:bob" }, builder =>
{
    builder.Append("notifications", "<div class='notification'>New message!</div>");
});
```

### Broadcast(Action\<ITurboStreamBuilder\> build)

Broadcasts Turbo Stream updates to **all** connected clients regardless of their subscriptions.

**Parameters:**
- `build` - An action that configures the stream updates to send

**Returns:** A `Task` that completes when the broadcast has been sent.

**Exceptions:**
- `ArgumentNullException` - When `build` is null

**Example:**

```csharp
// System-wide announcement
await _turbo.Broadcast(builder =>
{
    builder.Update("announcement", "<p>System maintenance at midnight</p>");
});
```

### Stream(string streamName, Action\<ITurboStreamBuilder\> build, string? excludedConnectionId)

Broadcasts Turbo Stream updates to all clients subscribed to the specified stream, excluding a specific connection.

**Parameters:**
- `streamName` - The name of the stream to broadcast to
- `build` - An action that configures the stream updates to send
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion

**Example:**

```csharp
var connectionId = HttpContext.GetSignalRConnectionId();
await _turbo.Stream("room:1", builder =>
{
    builder.Append("messages", "<div>New message</div>");
}, connectionId);
```

### Stream(IEnumerable\<string\> streamNames, Action\<ITurboStreamBuilder\> build, string? excludedConnectionId)

Broadcasts Turbo Stream updates to multiple streams, excluding a specific connection.

**Parameters:**
- `streamNames` - The names of the streams to broadcast to
- `build` - An action that configures the stream updates to send
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion

### Broadcast(Action\<ITurboStreamBuilder\> build, string? excludedConnectionId)

Broadcasts Turbo Stream updates to all connected clients, excluding a specific connection.

**Parameters:**
- `build` - An action that configures the stream updates to send
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion

### Stream(string streamName, Func\<ITurboStreamBuilder, Task\> buildAsync, string? excludedConnectionId)

Async version of Stream with connection exclusion.

**Parameters:**
- `streamName` - The name of the stream to broadcast to
- `buildAsync` - An async function that configures the stream updates to send
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion

### Stream(IEnumerable\<string\> streamNames, Func\<ITurboStreamBuilder, Task\> buildAsync, string? excludedConnectionId)

Async version of multi-stream broadcast with connection exclusion.

**Parameters:**
- `streamNames` - The names of the streams to broadcast to
- `buildAsync` - An async function that configures the stream updates to send
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion

### Broadcast(Func\<ITurboStreamBuilder, Task\> buildAsync, string? excludedConnectionId)

Async version of Broadcast with connection exclusion.

**Parameters:**
- `buildAsync` - An async function that configures the stream updates to send
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion

### StreamRefresh(string streamName, bool morph = false, bool preserveScroll = false)

Sends a Turbo Stream refresh action to a stream.

**Parameters:**
- `streamName` - The name of the stream to send the refresh to
- `morph` - When `true`, uses morphing for the refresh instead of full page replacement
- `preserveScroll` - When `true`, preserves scroll position during the refresh

**Example:**

```csharp
// Simple refresh
await _turbo.StreamRefresh("room:1");

// Morph refresh with scroll preservation
await _turbo.StreamRefresh("room:1", morph: true, preserveScroll: true);
```

### StreamRefresh(IEnumerable\<string\> streamNames, bool morph = false, bool preserveScroll = false)

Sends a Turbo Stream refresh action to multiple streams.

**Parameters:**
- `streamNames` - The names of the streams to send the refresh to
- `morph` - When `true`, uses morphing for the refresh instead of full page replacement
- `preserveScroll` - When `true`, preserves scroll position during the refresh

### BroadcastRefresh(bool morph = false, bool preserveScroll = false)

Sends a Turbo Stream refresh action to all connected clients.

**Parameters:**
- `morph` - When `true`, uses morphing for the refresh instead of full page replacement
- `preserveScroll` - When `true`, preserves scroll position during the refresh

### StreamRefresh(string streamName, string? excludedConnectionId, bool morph = false, bool preserveScroll = false)

Sends a Turbo Stream refresh action to a stream, excluding a specific connection.

**Parameters:**
- `streamName` - The name of the stream to send the refresh to
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion
- `morph` - When `true`, uses morphing for the refresh instead of full page replacement
- `preserveScroll` - When `true`, preserves scroll position during the refresh

**Example:**

```csharp
var connectionId = HttpContext.GetSignalRConnectionId();
await _turbo.StreamRefresh("room:1", connectionId, morph: true, preserveScroll: true);
```

### StreamRefresh(IEnumerable\<string\> streamNames, string? excludedConnectionId, bool morph = false, bool preserveScroll = false)

Sends a Turbo Stream refresh action to multiple streams, excluding a specific connection.

**Parameters:**
- `streamNames` - The names of the streams to send the refresh to
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion
- `morph` - When `true`, uses morphing for the refresh instead of full page replacement
- `preserveScroll` - When `true`, preserves scroll position during the refresh

### BroadcastRefresh(string? excludedConnectionId, bool morph = false, bool preserveScroll = false)

Sends a Turbo Stream refresh action to all connected clients, excluding a specific connection.

**Parameters:**
- `excludedConnectionId` - The SignalR connection ID to exclude, or null for no exclusion
- `morph` - When `true`, uses morphing for the refresh instead of full page replacement
- `preserveScroll` - When `true`, preserves scroll position during the refresh

## Common Patterns

### User-Specific Updates

Send updates to a specific user:

```csharp
public async Task<IActionResult> OnPostAddToCart(int productId)
{
    // Add to cart logic...

    await _turbo.Stream($"user:{User.Identity.Name}", builder =>
    {
        builder.Update("cart-count", $"<span>{cart.ItemCount}</span>");
        builder.Replace("cart-total", $"<span>{cart.Total:C}</span>");
    });

    return new NoContentResult();
}
```

### Resource-Specific Updates

Send updates to all users viewing a specific resource:

```csharp
public async Task<IActionResult> OnPostComment(int articleId, string comment)
{
    // Save comment logic...

    await _turbo.Stream($"article:{articleId}:comments", builder =>
    {
        builder.Append("comments-list", RenderComment(newComment));
    });

    return new NoContentResult();
}
```

### Multiple Actions in One Update

Chain multiple actions for atomic updates:

```csharp
await _turbo.Stream("order:456", builder =>
{
    builder
        .Replace("order-status", "<span class='status-shipped'>Shipped</span>")
        .Append("order-history", "<li>Order shipped on 2024-01-15</li>")
        .Remove("cancel-order-button");
});
```

## See Also

- [ITurboStreamBuilder](ITurboStreamBuilder.md) - Building stream actions
- [TurboOptions](TurboOptions.md) - Configuration options
- [Turbo Streams Guide](../guides/turbo-streams.md) - Complete guide
