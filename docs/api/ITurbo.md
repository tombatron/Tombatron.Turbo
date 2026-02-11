# ITurbo Interface

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
