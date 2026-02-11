# Tag Helpers

Tombatron.Turbo provides tag helpers for rendering Turbo elements in Razor views.

## Setup

Add the tag helpers to your `_ViewImports.cshtml`:

```razor
@addTagHelper *, Tombatron.Turbo
```

## turbo-frame

Renders a `<turbo-frame>` element for partial page updates.

### Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `id` | `string` | **Required.** Unique identifier for the frame |
| `src` | `string` | URL to load content from lazily or on refresh |
| `loading` | `string` | Loading behavior: `"eager"` (default) or `"lazy"` |
| `disabled` | `bool` | When true, frame won't intercept navigation |
| `target` | `string` | Navigation target: `"_top"` breaks out of frame |
| `autoscroll` | `bool` | Restore scroll position after navigation |

### Examples

**Basic frame:**
```html
<turbo-frame id="cart-items">
    <!-- Content here -->
</turbo-frame>
```

**Lazy-loaded frame:**
```html
<turbo-frame id="comments" src="/posts/1/comments" loading="lazy">
    <p>Loading comments...</p>
</turbo-frame>
```

**Frame with refresh link:**
```html
<turbo-frame id="notifications" src="/notifications?handler=List">
    @foreach (var notification in Model.Notifications)
    {
        <div>@notification.Message</div>
    }
</turbo-frame>

<a href="/notifications?handler=List" data-turbo-frame="notifications">
    Refresh Notifications
</a>
```

**Disabled frame:**
```html
<turbo-frame id="preview" disabled="true">
    <!-- Links and forms won't be intercepted -->
</turbo-frame>
```

**Break out of frame:**
```html
<turbo-frame id="login-form" target="_top">
    <!-- Form submission will navigate the whole page -->
</turbo-frame>
```

## turbo

Renders a `<turbo-stream-source-signalr>` element for real-time stream subscriptions.

### Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `stream` | `string` | Stream name(s) to subscribe to (comma-separated) |
| `hub-url` | `string` | SignalR hub URL (defaults to `TurboOptions.HubPath`) |
| `id` | `string` | Optional element ID |

### Examples

**Subscribe to a specific stream:**
```html
<turbo stream="notifications"></turbo>
```

**Subscribe to multiple streams:**
```html
<turbo stream="notifications,updates,alerts"></turbo>
```

**Auto-generate stream based on user:**
```html
<!-- For authenticated users: user:{userId} -->
<!-- For anonymous users: session:{sessionId} -->
<turbo></turbo>
```

**Custom hub URL:**
```html
<turbo stream="chat" hub-url="/my-signalr-hub"></turbo>
```

### Generated Output

For a single stream:
```html
<turbo-stream-source-signalr stream="notifications" hub-url="/turbo-hub">
</turbo-stream-source-signalr>
```

For multiple streams:
```html
<div data-turbo-streams="true">
    <turbo-stream-source-signalr stream="notifications" hub-url="/turbo-hub"></turbo-stream-source-signalr>
    <turbo-stream-source-signalr stream="updates" hub-url="/turbo-hub"></turbo-stream-source-signalr>
</div>
```

## Best Practices

### Frame IDs

- Use descriptive, unique IDs: `cart-items`, `user-profile`, `comment-list`
- For dynamic content, use prefixed IDs: `item_123`, `comment_456`
- IDs must be unique across the entire page

### Stream Names

- Use namespaced names: `user:123`, `room:456`, `order:789`
- Keep names short but descriptive
- Avoid special characters except `:`, `-`, and `_`

### Performance

- Use `loading="lazy"` for below-the-fold content
- Prefer specific frame updates over full page reloads
- Minimize the number of active stream subscriptions

## See Also

- [Turbo Frames Guide](../guides/turbo-frames.md)
- [Turbo Streams Guide](../guides/turbo-streams.md)
