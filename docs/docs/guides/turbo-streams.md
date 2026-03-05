---
title: "Turbo Streams Guide"
sidebar_label: "Turbo Streams"
sidebar_position: 2
description: "Real-time updates by pushing DOM changes from the server to connected clients via SignalR."
---

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

<!-- Turbo SignalR adapter (bundled with SignalR) -->
<script src="_content/Tombatron.Turbo/dist/turbo-signalr.bundled.min.js"></script>
```

**Alternative: Using npm or CDN separately**

```html
<!-- Option 2: From npm/CDN (requires separate SignalR) -->
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8/dist/browser/signalr.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/@tombatron/turbo-signalr/dist/turbo-signalr.js"></script>
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

## Morphing

Turbo v8 introduces **morphing** as an alternative to full DOM replacement. When enabled, Turbo uses [idiomorph](https://github.com/bigskysoftware/idiomorph) to intelligently diff and patch the DOM, preserving state like form inputs, focus, and scroll positions.

### Morph on Replace and Update

The `Replace` and `Update` actions accept an optional `morph` parameter:

```csharp
// Standard replace — removes the old element and inserts the new one
builder.Replace("user-card", newCardHtml);

// Morph replace — patches the existing element in place
builder.Replace("user-card", newCardHtml, morph: true);

// Morph update — patches inner content while preserving element state
builder.Update("product-list", newListHtml, morph: true);
```

This adds a `method="morph"` attribute to the generated `<turbo-stream>` tag:

```html
<turbo-stream action="replace" method="morph" target="user-card">
  <template><div id='user-card'>Updated Card</div></template>
</turbo-stream>
```

Morph is especially useful for elements with complex state (e.g., forms with unsaved input, elements with CSS transitions) where a full replacement would be disruptive.

### Morph with CSS Selector Targeting

The `ReplaceAll` and `UpdateAll` methods also support morph:

```csharp
builder.ReplaceAll(".status-card", updatedCardHtml, morph: true);
builder.UpdateAll(".price-tag", newPriceHtml, morph: true);
```

## Targeting Multiple Elements

The `*All` methods let you target multiple elements using a CSS selector instead of a single DOM ID. The selector is passed via the `targets` attribute (plural) on the `<turbo-stream>` tag.

```csharp
// Update all elements with class "price" at once
builder.UpdateAll(".price", "$9.99");

// Remove all dismissed notifications
builder.RemoveAll(".notification.dismissed");

// Append to every feed container on the page
builder.AppendAll("[data-feed]", "<div class='entry'>New event</div>");
```

**Generated HTML:**
```html
<turbo-stream action="update" targets=".price">
  <template>$9.99</template>
</turbo-stream>
```

All seven `*All` methods mirror the single-target actions:

| Method | Description |
|--------|-------------|
| `AppendAll(targets, html)` | Append to all matching elements |
| `PrependAll(targets, html)` | Prepend to all matching elements |
| `ReplaceAll(targets, html, morph)` | Replace all matching elements |
| `UpdateAll(targets, html, morph)` | Update inner content of all matching elements |
| `RemoveAll(targets)` | Remove all matching elements |
| `BeforeAll(targets, html)` | Insert before all matching elements |
| `AfterAll(targets, html)` | Insert after all matching elements |

## Page Refresh

The `Refresh` action tells connected clients to re-fetch and re-render the current page. This is useful after server-side changes that affect the whole page layout rather than individual elements.

### Using the Builder

```csharp
await _turbo.Stream("room:1", builder =>
{
    builder.Refresh();
});

// With morph and scroll preservation
await _turbo.Stream("room:1", builder =>
{
    builder.Refresh(morph: true, preserveScroll: true);
});
```

### Convenience Methods

`ITurbo` provides `StreamRefresh` and `BroadcastRefresh` for the common case where a refresh is the only action:

```csharp
// Refresh a single stream
await _turbo.StreamRefresh("room:1");

// Morph refresh with scroll preservation
await _turbo.StreamRefresh("room:1", morph: true, preserveScroll: true);

// Refresh all connected clients
await _turbo.BroadcastRefresh(morph: true, preserveScroll: true);

// With connection exclusion
var connectionId = HttpContext.GetSignalRConnectionId();
await _turbo.StreamRefresh("room:1", connectionId, morph: true, preserveScroll: true);
```

## Meta Tag Configuration

Use the `<turbo-meta>` tag helper to set page-level defaults for Turbo refresh behavior. Place it in the `<head>` of your layout:

```html
<head>
    <turbo-meta refresh-method="Morph" refresh-scroll="Preserve" />
</head>
```

This generates:
```html
<meta name="turbo-refresh-method" content="morph">
<meta name="turbo-refresh-scroll" content="preserve">
```

When these meta tags are present, all Turbo page refreshes on that page will use morphing and preserve scroll position by default. See the [Tag Helpers reference](../api/TagHelpers.md#turbo-meta) for the full list of attributes.

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

## Excluding the Originator

When a user submits a form that triggers a Turbo Stream broadcast, they receive both the HTTP response and the SignalR broadcast — a duplicate update. To prevent this, you can exclude the originator's SignalR connection from the broadcast.

### How It Works

1. The JavaScript adapter automatically sends an `X-SignalR-Connection-Id` header on every Turbo fetch request (via the `turbo:before-fetch-request` event). Each browser tab holds its own connection ID in memory, so this works correctly with multiple tabs open.
2. The `TurboFrameMiddleware` reads this header and stores it in `HttpContext.Items`
3. You read it with `HttpContext.GetSignalRConnectionId()` and pass it to the broadcast method
4. SignalR uses `GroupExcept` / `AllExcept` to skip that connection

### Usage

```csharp
public async Task<IActionResult> OnPostSendMessage(int roomId, string content)
{
    var message = SaveMessage(roomId, content);

    var connectionId = HttpContext.GetSignalRConnectionId();
    await _turbo.Stream($"room:{roomId}", builder =>
    {
        builder.Append("messages", $"<div>{message.Text}</div>");
    }, connectionId);

    return new NoContentResult();
}
```

The `excludedConnectionId` parameter is `string?`. Passing `null` (e.g., on the initial page load before the SignalR connection is established) simply broadcasts to all subscribers with no exclusion.

### Difference vs. X-Turbo-Request-Id

| Mechanism | Scope | How it works |
|-----------|-------|-------------|
| `excludedConnectionId` | Any stream action | Server-side: SignalR never sends the message to that connection |
| `X-Turbo-Request-Id` | `refresh` action only | Client-side: Turbo.js ignores the refresh if the request ID matches |

Both can be used together. The refresh overloads (`StreamRefresh`, `BroadcastRefresh`) automatically include the request ID and accept an optional `excludedConnectionId`:

```csharp
var connectionId = HttpContext.GetSignalRConnectionId();
await _turbo.StreamRefresh("room:1", connectionId);
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
