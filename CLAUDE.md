# Tombatron.Turbo

Hotwire Turbo + Stimulus integration for ASP.NET Core (.NET 10). Server-side rendering with real-time updates via SignalR.
NuGet: `Tombatron.Turbo`, `Tombatron.Turbo.Stimulus` | npm: `@tombatron/turbo-signalr`

## Setup Checklist

```csharp
// Program.cs
builder.Services.AddTurbo(); // 1. Register services
// or with options:
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";
    options.UseSignedStreamNames = true;
});

builder.Services.AddStimulus(); // optional, for Stimulus controller discovery

var app = builder.Build();

app.UseRouting();
app.UseTurbo();      // 2. Add middleware (after UseRouting)
app.MapTurboHub();   // 3. Map SignalR hub
```

```html
<!-- _ViewImports.cshtml -->
@addTagHelper *, Tombatron.Turbo         <!-- 4. Register tag helpers -->

<!-- _Layout.cshtml <head> -->
<turbo-scripts />                         <!-- 5. Emit JS (importmap mode by default) -->
<turbo-meta refresh-method="Morph" refresh-scroll="Preserve" />  <!-- optional -->
```

## Turbo Frames

### Basic pattern: partial view in a frame

```html
<!-- Views/Shared/_CartItems.cshtml -->
<turbo-frame id="cart-items">
    @foreach (var item in Model.Items)
    {
        <div id="item_@item.Id">@item.Name</div>
    }
</turbo-frame>
```

```csharp
// Controller or PageModel
public IActionResult OnGetItems()
{
    if (HttpContext.IsTurboFrameRequest())
        return Partial(Partials.CartItems.Name, Model);

    return RedirectToPage(); // non-Turbo fallback
}
```

**Critical rule:** The response MUST contain a `<turbo-frame>` with the same `id` the client requested. Mismatched IDs cause a silent failure.

### Lazy loading

```html
<turbo-frame id="dashboard-stats" src="/Dashboard?handler=Stats" loading="lazy">
    Loading...
</turbo-frame>
```

### Dynamic IDs for lists

```html
<turbo-frame id="item_@item.Id">...</turbo-frame>
```

### HttpContext extensions

| Method | Returns | Purpose |
|--------|---------|---------|
| `IsTurboFrameRequest()` | `bool` | Any Turbo Frame request |
| `IsTurboFrameRequest("id")` | `bool` | Frame request for specific ID |
| `IsTurboFrameRequestWithPrefix("prefix_")` | `bool` | Frame ID starts with prefix |
| `GetTurboFrameId()` | `string?` | The requested frame ID |
| `IsTurboStreamRequest()` | `bool` | Accept header contains `text/vnd.turbo-stream.html` |
| `GetSignalRConnectionId()` | `string?` | Client's SignalR connection ID (for exclusion) |
| `GetTurboRequestId()` | `string?` | X-Turbo-Request-Id (for refresh suppression) |

## Turbo Streams -- Real-time Updates

### A) Subscribe in Razor

```html
<turbo stream="notifications"></turbo>
<turbo stream="notifications,updates"></turbo>  <!-- multiple streams -->
<turbo></turbo>  <!-- auto: user:{id} or session:{id} -->
```

### B) Broadcast from server (inject ITurbo)

```csharp
// Single stream
await turbo.Stream("cart:123", builder =>
{
    builder.Replace("cart-total", "<span>$99.99</span>");
});

// Multiple streams
await turbo.Stream(new[] { "user:alice", "user:bob" }, builder =>
{
    builder.Append("notifications", "<div>New message!</div>");
});

// All connected clients
await turbo.Broadcast(builder =>
{
    builder.Update("announcement", "<p>System maintenance at midnight</p>");
});

// Exclude originator
var connId = HttpContext.GetSignalRConnectionId();
await turbo.Stream("room:123", builder =>
{
    builder.Append("messages", html);
}, excludedConnectionId: connId);

// Broadcast also supports excludedConnectionId
await turbo.Broadcast(builder =>
{
    builder.Update("announcement", html);
}, excludedConnectionId: connId);

// Refresh (uses X-Turbo-Request-Id for originator suppression)
await turbo.StreamRefresh("dashboard");
await turbo.StreamRefresh(new[] { "page:1", "page:2" });
await turbo.BroadcastRefresh();
// All Stream, Broadcast, and Refresh methods accept optional excludedConnectionId
```

### Async with partial rendering

```csharp
await turbo.Stream("room:123", async builder =>
{
    await builder.AppendAsync("messages", Partials.Message, messageModel);
});
```

### Builder actions

| Method | *All variant | Description |
|--------|-------------|-------------|
| `Append(target, html)` | `AppendAll(targets, html)` | Add to end of target |
| `Prepend(target, html)` | `PrependAll(targets, html)` | Add to start of target |
| `Replace(target, html, morph?)` | `ReplaceAll(targets, html, morph?)` | Replace entire element |
| `Update(target, html, morph?)` | `UpdateAll(targets, html, morph?)` | Replace inner content |
| `Remove(target)` | `RemoveAll(targets)` | Remove from DOM |
| `Before(target, html)` | `BeforeAll(targets, html)` | Insert before element |
| `After(target, html)` | `AfterAll(targets, html)` | Insert after element |
| `Refresh(requestId?)` | -- | Trigger page refresh |

Single-target methods use DOM IDs. `*All` variants use CSS selectors.
`morph: true` preserves DOM state during replace/update.

### Async partial variants (extension methods)

`AppendAsync`, `PrependAsync`, `ReplaceAsync`, `UpdateAsync`, `BeforeAsync`, `AfterAsync` -- all take `(target, PartialTemplate<TModel>, model)`. Must use the `Func<ITurboStreamBuilder, Task>` overload of `Stream()`/`Broadcast()`.

## Form Validation

Return HTTP 422 for validation failures inside frames. Turbo replaces the frame content in-place.

```csharp
// MVC
if (!ModelState.IsValid)
{
    Response.StatusCode = 422;
    return Partial(Partials.EditForm.Name, model);
}

// Minimal API
if (!IsValid(model))
    return TurboResults.ValidationFailure(Partials.EditForm, model);
```

## Tag Helper Reference

### `<turbo-frame>`

| Attribute | Type | Description |
|-----------|------|-------------|
| `id` | `string` | Required. Unique frame identifier |
| `src` | `string` | URL to load content from |
| `loading` | `Eager\|Lazy` | When to load (default: Eager) |
| `refresh` | `Replace\|Morph` | How to update during page refresh |
| `target` | `string` | Navigation target (`_top` to break out) |
| `disabled` | `bool` | Disable frame navigation |
| `autoscroll` | `bool` | Restore scroll position |

### `<turbo>`

| Attribute | Type | Description |
|-----------|------|-------------|
| `stream` | `string` | Stream name(s), comma-separated. Auto-generates if omitted |
| `hub-url` | `string` | Custom SignalR hub URL (default: from TurboOptions) |
| `id` | `string` | Optional element ID |

### `<turbo-meta />`

| Attribute | Values | Description |
|-----------|--------|-------------|
| `refresh-method` | `Morph\|Replace` | Page refresh strategy |
| `refresh-scroll` | `Preserve\|Reset` | Scroll behavior on refresh |
| `cache-control` | `no-cache` etc. | Turbo cache directive |
| `visit-control` | `reload` etc. | Turbo visit directive |

### `<turbo-scripts />`

| Attribute | Values | Description |
|-----------|--------|-------------|
| `mode` | `Importmap\|Traditional` | Script rendering mode (default: Importmap) |

## Configuration

`TurboOptions` properties (set via `AddTurbo(options => { ... })`):

| Property | Default | Description |
|----------|---------|-------------|
| `HubPath` | `"/turbo-hub"` | SignalR hub endpoint path |
| `UseSignedStreamNames` | `true` | Cryptographically sign stream subscriptions |
| `SignedStreamNameExpiration` | `24 hours` | Token expiration (`null` = no expiration) |
| `AddVaryHeader` | `true` | Auto-add `Vary: Turbo-Frame` header |
| `DefaultUserStreamPattern` | `"user:{0}"` | Stream name for authenticated users |
| `DefaultSessionStreamPattern` | `"session:{0}"` | Stream name for anonymous sessions |
| `EnableAutoReconnect` | `true` | Auto-reconnect SignalR clients |
| `MaxReconnectAttempts` | `5` | Max reconnection attempts |
| `ImportMap` | (preconfigured) | Import map entries for JS modules |

## Authorization

Stream subscriptions are secured by default via signed stream names (ASP.NET Core Data Protection API). If the server rendered the `<turbo stream="...">` tag, the client is implicitly authorized.

For custom logic, implement `ITurboStreamAuthorization`:

```csharp
public class CustomAuthorization : ITurboStreamAuthorization
{
    public bool CanSubscribe(ClaimsPrincipal? user, string streamName)
    {
        if (streamName.StartsWith("user:"))
        {
            string userId = streamName.Substring(5);
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value == userId;
        }
        return true;
    }
}

// Register before AddTurbo:
builder.Services.AddSingleton<ITurboStreamAuthorization, CustomAuthorization>();
```

## Minimal API Support

`TurboResults` provides `IResult` factory methods for minimal APIs:

```csharp
app.MapGet("/items/{id}", (int id) =>
    TurboResults.Partial("_ItemDetail", new { Id = id }));

app.MapPost("/items", (ItemModel model) =>
    !IsValid(model)
        ? TurboResults.ValidationFailure("_EditForm", model)
        : TurboResults.Partial("_ItemDetail", model));

// Also works with PartialTemplate / PartialTemplate<TModel> from the source generator:
app.MapGet("/items/{id}", (int id, ItemModel item) =>
    TurboResults.Partial(Partials.ItemDetail, item));
```

All `Partial()` and `ValidationFailure()` overloads also accept `PartialTemplate` (no model) or `PartialTemplate<TModel>` (typed model), and string-based overloads work with or without a model parameter.

The `Tombatron.Turbo.SourceGenerator` package generates a `Partials` class (`Tombatron.Turbo.Generated.Partials`) from `_*.cshtml` files, providing compile-time `PartialTemplate<TModel>` instances for use with `TurboResults.Partial()` and async stream builder methods.

## Stimulus

Requires NuGet package `Tombatron.Turbo.Stimulus`.

```csharp
builder.Services.AddStimulus(); // or with options:
builder.Services.AddStimulus(options =>
{
    options.ControllersPath = "controllers";        // relative to wwwroot
    options.IndexEndpointPath = "/_stimulus/controllers/index.js";
    options.StimulusCdnUrl = "https://unpkg.com/@hotwired/stimulus@3.2.2/dist/stimulus.js";
    options.EnableHotReload = null;                  // null = auto (enabled in Development)
});
```

Place controllers in `wwwroot/controllers/` named `*_controller.js` or `*-controller.js`. They are auto-discovered and registered. Naming: `hello_controller.js` becomes identifier `"hello"`, `admin/user_profile_controller.js` becomes `"admin--user-profile"`.

For any client-side interactivity, create a Stimulus controller rather than using inline scripts or standalone JS files.

```html
<!-- Usage -->
<div data-controller="hello">
    <input data-hello-target="name" type="text">
    <button data-action="click->hello#greet">Greet</button>
</div>
```

## Common Gotchas

- **Missing matching frame ID**: Turbo Frame responses must contain a `<turbo-frame id="...">` matching the request. No match = silent failure.
- **Wrong status code on validation**: Use HTTP 422, not 200 or 400. Turbo only processes frame replacements on 422 for form errors.
- **Middleware ordering**: `UseTurbo()` must come after `UseRouting()`. The middleware reads headers and populates `HttpContext.Items`.
- **Signed stream expiration**: Default 24h. Long-lived pages need page refresh to get new tokens.
- **Forgetting `@addTagHelper`**: Tag helpers silently pass through as plain HTML without registration.
- **Async builder without async overload**: `AppendAsync`/etc. require the `Func<ITurboStreamBuilder, Task>` overload of `Stream()`. Using the `Action<ITurboStreamBuilder>` overload throws `InvalidOperationException`.
- **`<turbo-scripts />` missing**: No JS loads, Turbo Drive/Frames/Streams all fail silently.
- **Stimulus controller naming**: Files must end with `_controller.js` or `-controller.js` to be discovered.

## Hotwire Concepts Summary

**Turbo Drive** intercepts link clicks and form submissions, replacing the `<body>` via fetch instead of full page loads. Enabled automatically by `<turbo-scripts />`.

**Turbo Frames** decompose pages into independently-updatable sections. A `<turbo-frame>` scopes navigation -- clicks/forms inside only update that frame. The server response must contain the matching frame.

**Turbo Streams** deliver real-time DOM updates over SignalR. Eight actions (append, prepend, replace, update, remove, before, after, refresh) target elements by DOM ID or CSS selector. Subscribe with `<turbo stream="...">`.

**Stimulus** is a lightweight JS framework that connects controllers to DOM elements via `data-*` attributes. It complements server-rendered HTML -- use it for small behaviors (toggles, validations, clipboard), not for building SPAs.
