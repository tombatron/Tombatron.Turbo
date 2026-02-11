# Turbo Streams Guide

Turbo Streams enable real-time updates by pushing DOM changes from the server to connected clients via SignalR. Unlike Turbo Frames (which require user interaction), Streams can update any element at any time.

## How It Works

1. Client connects to the SignalR hub and subscribes to streams
2. Server broadcasts updates using `ITurbo.Stream()`
3. Client receives the update and applies DOM changes
4. Turbo.js renders the changes automatically

## Setup

### 1. Configure Services

```csharp
// Program.cs
builder.Services.AddTurbo();
```

### 2. Add Middleware and Hub

```csharp
// Program.cs
app.UseRouting();
app.UseTurbo();
app.MapRazorPages();
app.MapTurboHub(); // Map the SignalR hub
```

### 3. Include Required Scripts

Add to your layout:

```html
<!-- Turbo.js from CDN -->
<script type="module" src="https://cdn.jsdelivr.net/npm/@hotwired/turbo@8/dist/turbo.es2017-esm.min.js"></script>

<!-- SignalR from CDN -->
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8/dist/browser/signalr.min.js"></script>

<!-- Turbo SignalR adapter (inline or from your bundle) -->
<script>
// Custom element for SignalR stream connections
// See samples for full implementation
</script>
```

### 4. Subscribe to Streams

Add to your page:

```html
<turbo-stream-source-signalr stream="notifications" hub-url="/turbo-hub">
</turbo-stream-source-signalr>
```

Or use the tag helper:

```html
<turbo stream="notifications"></turbo>
```

### 5. Broadcast Updates

Inject and use `ITurbo`:

```csharp
public class NotificationsController : Controller
{
    private readonly ITurbo _turbo;

    public NotificationsController(ITurbo turbo)
    {
        _turbo = turbo;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string message)
    {
        // Save notification...

        await _turbo.Stream("notifications", builder =>
        {
            builder.Append("notification-list",
                $"<div class='notification'>{message}</div>");
        });

        return Ok();
    }
}
```

## Stream Actions

### Append

Add content to the end of an element:

```csharp
builder.Append("messages", "<div class='message'>Hello!</div>");
```

### Prepend

Add content to the beginning of an element:

```csharp
builder.Prepend("activity-log", "<div class='entry'>New event</div>");
```

### Replace

Replace an element entirely:

```csharp
builder.Replace("user-status",
    "<span id='user-status' class='online'>Online</span>");
```

> Remember to include the ID in the replacement content if you'll target it again.

### Update

Replace an element's inner content:

```csharp
builder.Update("counter", "42");
builder.Update("cart-total", "$149.99");
```

### Remove

Remove an element from the DOM:

```csharp
builder.Remove("item-123");
builder.Remove("temporary-banner");
```

### Before / After

Insert content adjacent to an element:

```csharp
builder.Before("submit-button", "<div class='warning'>Please review</div>");
builder.After("header", "<div class='announcement'>New feature!</div>");
```

## Stream Naming Conventions

Use descriptive, namespaced names:

| Pattern | Use Case |
|---------|----------|
| `user:{userId}` | User-specific notifications |
| `room:{roomId}` | Chat rooms |
| `order:{orderId}` | Order status updates |
| `resource:{type}:{id}` | Resource-specific updates |
| `broadcast` | System-wide announcements |

### Examples

```csharp
// User notifications
await _turbo.Stream($"user:{userId}", ...)

// Chat room
await _turbo.Stream($"room:{roomId}", ...)

// Order updates (notify customer and admin)
await _turbo.Stream(new[] {
    $"user:{customerId}",
    $"admin:orders"
}, ...)

// Everyone
await _turbo.Broadcast(...)
```

## Common Patterns

### Real-Time Notifications

```csharp
public async Task NotifyUser(string userId, string message)
{
    await _turbo.Stream($"user:{userId}", builder =>
    {
        builder.Prepend("notifications", $@"
            <div class='notification' id='notif-{Guid.NewGuid()}'>
                {message}
                <button onclick='this.parentElement.remove()'>Dismiss</button>
            </div>");
    });
}
```

### Live Counters

```csharp
public async Task UpdateOnlineCount(int count)
{
    await _turbo.Broadcast(builder =>
    {
        builder.Update("online-count", count.ToString());
    });
}
```

### Chat Messages

```csharp
public async Task SendMessage(string roomId, ChatMessage message)
{
    await _turbo.Stream($"room:{roomId}", builder =>
    {
        builder.Append("chat-messages", $@"
            <div class='message'>
                <strong>{message.Author}:</strong> {message.Text}
                <time>{message.Timestamp:HH:mm}</time>
            </div>");
    });
}
```

### Shopping Cart Updates

```csharp
public async Task<IActionResult> AddToCart(int productId)
{
    var cart = AddItemToCart(productId);

    await _turbo.Stream($"user:{User.Identity.Name}", builder =>
    {
        builder
            .Update("cart-count", cart.ItemCount.ToString())
            .Update("cart-total", cart.Total.ToString("C"));
    });

    return new NoContentResult();
}
```

### Form Response with Streams

Return a Turbo Stream response from a form submission:

```csharp
public IActionResult OnPostAddItem(string name)
{
    var item = CreateItem(name);

    if (Request.Headers.Accept.ToString().Contains("text/vnd.turbo-stream.html"))
    {
        return Content($@"
            <turbo-stream action=""append"" target=""item-list"">
                <template>
                    <div id=""item-{item.Id}"">{item.Name}</div>
                </template>
            </turbo-stream>
            <turbo-stream action=""update"" target=""item-count"">
                <template>{GetItemCount()}</template>
            </turbo-stream>
        ", "text/vnd.turbo-stream.html");
    }

    return RedirectToPage();
}
```

## Multiple Streams

### Subscribe to Multiple

```html
<turbo stream="notifications,updates,alerts"></turbo>
```

### Broadcast to Multiple

```csharp
await _turbo.Stream(new[] { "stream1", "stream2" }, builder => ...);
```

## Connection Status

The SignalR adapter dispatches events you can listen for:

```javascript
window.addEventListener('turbo-signalr:connected', () => {
    console.log('Connected to Turbo hub');
});

window.addEventListener('turbo-signalr:disconnected', () => {
    console.log('Disconnected from Turbo hub');
});

window.addEventListener('turbo-signalr:reconnecting', () => {
    console.log('Attempting to reconnect...');
});
```

## Best Practices

### 1. Use Specific Targets

Target specific elements rather than large containers:

```csharp
// Good - specific target
builder.Update("order-status", "Shipped");

// Avoid - replaces too much content
builder.Replace("order-container", largeHtmlBlob);
```

### 2. Chain Related Updates

Group related updates in a single broadcast:

```csharp
await _turbo.Stream($"user:{userId}", builder =>
{
    builder
        .Update("cart-count", count.ToString())
        .Update("cart-total", total.ToString("C"))
        .Remove("empty-cart-message");
});
```

### 3. Handle Disconnections

Design your UI to work without real-time updates:

- Show connection status indicators
- Provide manual refresh options
- Ensure forms work with redirects as fallback

### 4. Minimize Broadcast Scope

Use targeted streams instead of broadcasting to everyone:

```csharp
// Good - only affected user receives update
await _turbo.Stream($"user:{userId}", ...);

// Use sparingly - everyone receives update
await _turbo.Broadcast(...);
```

## See Also

- [ITurbo](../api/ITurbo.md) - Service API reference
- [ITurboStreamBuilder](../api/ITurboStreamBuilder.md) - Builder API reference
- [Authorization](authorization.md) - Securing stream subscriptions
- [Tag Helpers](../api/TagHelpers.md) - Stream tag helper
