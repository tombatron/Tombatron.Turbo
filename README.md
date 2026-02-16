# Tombatron.Turbo

[![Build and Test](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml/badge.svg)](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Tombatron.Turbo.svg)](https://www.nuget.org/packages/Tombatron.Turbo/)
[![npm](https://img.shields.io/npm/v/@tombatron/turbo-signalr.svg)](https://www.npmjs.com/package/@tombatron/turbo-signalr)

Hotwire Turbo for ASP.NET Core with SignalR-powered real-time streams.

## Features

- **Turbo Frames**: Partial page updates with automatic `Turbo-Frame` header detection
- **Turbo Streams**: Real-time updates via SignalR with targeted and broadcast support
- **Source Generator**: Compile-time strongly-typed partial references
- **Minimal API Support**: Return partials from Minimal API endpoints with `TurboResults`
- **Simple Architecture**: Check for `Turbo-Frame` header, return partial or redirect
- **Zero JavaScript Configuration**: Works out of the box with Turbo.js

## Installation

**NuGet (ASP.NET Core server package):**

```bash
dotnet add package Tombatron.Turbo
```

**NuGet (Source generator for strongly-typed partials, optional):**

```bash
dotnet add package Tombatron.Turbo.SourceGenerator
```

**npm (JavaScript client library):**

```bash
npm install @tombatron/turbo-signalr
```

## Quick Start

### 1. Add Turbo Services

```csharp
// Program.cs
builder.Services.AddTurbo();

// Or with import map configuration:
builder.Services.AddTurbo(options =>
{
    options.ImportMap.Pin("@hotwired/stimulus",
        "https://unpkg.com/@hotwired/stimulus@3.2.2/dist/stimulus.js", preload: true);
    options.ImportMap.Pin("controllers/hello", "/js/controllers/hello_controller.js");
});
```

### 2. Use Turbo Middleware

```csharp
// Program.cs
app.UseRouting();
app.UseTurbo();
app.MapRazorPages();
app.MapTurboHub(); // For Turbo Streams
```

### 3. Add Tag Helpers

```razor
@* _ViewImports.cshtml *@
@addTagHelper *, Tombatron.Turbo
```

### 4. Create a Turbo Frame with a Partial

Create a partial view for your frame content:

```html
<!-- Pages/Shared/_CartItems.cshtml -->
<turbo-frame id="cart-items">
    @foreach (var item in Model.Items)
    {
        <div>@item.Name - @item.Price</div>
    }
</turbo-frame>
```

Use the partial in your page:

```html
<!-- Pages/Cart/Index.cshtml -->
<h1>Shopping Cart</h1>
<partial name="_CartItems" model="Model" />
```

### 5. Handle Frame Requests in Your Page Model

```csharp
public class CartModel : PageModel
{
    public List<CartItem> Items { get; set; }

    public void OnGet()
    {
        Items = GetCartItems();
    }

    public IActionResult OnGetRefresh()
    {
        Items = GetCartItems();

        // For Turbo-Frame requests, return just the partial
        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_CartItems", this);
        }

        // For regular requests, redirect to the full page
        return RedirectToPage();
    }
}
```

### 6. Link to the Handler

```html
<turbo-frame id="cart-items" src="/Cart?handler=Refresh">
    Loading...
</turbo-frame>

<!-- Or use a button/link -->
<a href="/Cart?handler=Refresh" data-turbo-frame="cart-items">
    Refresh Cart
</a>
```

## Turbo Streams (Real-Time Updates)

### Send Updates to a Stream

```csharp
public class CartController : Controller
{
    private readonly ITurbo _turbo;

    public CartController(ITurbo turbo)
    {
        _turbo = turbo;
    }

    [HttpPost]
    public async Task<IActionResult> AddItem(int itemId)
    {
        // Add item to cart...

        // Send update to the user's stream
        await _turbo.Stream($"user:{User.Identity.Name}", builder =>
        {
            builder.Update("cart-total", $"<span>${cart.Total}</span>");
        });

        return Ok();
    }
}
```

### Broadcast to All Connected Clients

```csharp
// Send updates to every connected client
await _turbo.Broadcast(builder =>
{
    builder.Update("active-users", $"<span>{count}</span>");
});
```

### Render Partials in Streams

Use the async overload to render Razor partials directly in stream updates:

```csharp
await _turbo.Stream($"room:{roomId}", async builder =>
{
    await builder.AppendAsync("messages", "_Message", message);
});
```

With the source generator, you get strongly-typed partial references:

```csharp
await _turbo.Stream($"room:{roomId}", async builder =>
{
    await builder.AppendAsync("messages", Partials.Message, message);
});
```

### Include the Client Scripts

```html
<!-- In your layout: renders Turbo.js + SignalR adapter script tags -->
<turbo-scripts />

<!-- Or use import maps: -->
<turbo-scripts mode="Importmap" />
```

The `<turbo-scripts>` tag helper automatically includes Turbo.js and the SignalR bridge.
In **Traditional** mode (default), it renders standard `<script>` tags.
In **Importmap** mode, it renders a `<script type="importmap">` block with module preloads.

Configure additional modules (e.g. Stimulus) via `ImportMap.Pin()` in `Program.cs`:

```csharp
builder.Services.AddTurbo(options =>
{
    options.ImportMap.Pin("@hotwired/stimulus",
        "https://unpkg.com/@hotwired/stimulus@3.2.2/dist/stimulus.js", preload: true);
});
```

### Subscribe to Streams in Your View

```html
<!-- Using the turbo tag helper -->
<turbo stream="notifications"></turbo>

<!-- Or directly -->
<turbo-stream-source-signalr stream="user:@User.Identity.Name" hub-url="/turbo-hub">
</turbo-stream-source-signalr>

<div id="cart-total">$0.00</div>
```

## Stream Actions

```csharp
await _turbo.Stream("notifications", builder =>
{
    builder
        .Append("list", "<div>New item</div>")    // Add to end
        .Prepend("list", "<div>First</div>")      // Add to beginning
        .Replace("item-1", "<div>Updated</div>")  // Replace element
        .Update("count", "42")                     // Update inner content
        .Remove("old-item")                        // Remove element
        .Before("btn", "<div>Before</div>")       // Insert before
        .After("btn", "<div>After</div>");        // Insert after
});
```

## Minimal API Support

Use `TurboResults` to return partials from Minimal API endpoints:

```csharp
app.MapGet("/cart/items", (HttpContext ctx) =>
{
    if (ctx.IsTurboFrameRequest())
    {
        return TurboResults.Partial("_CartItems", model);
    }
    return Results.Redirect("/cart");
});
```

## Source Generator

The `Tombatron.Turbo.SourceGenerator` package scans your `_*.cshtml` partial views at compile time and generates an `internal Partials` static class with strongly-typed references:

```csharp
// Generated from _Message.cshtml with @model ChatMessage
internal static PartialTemplate<ChatMessage> Message { get; }
    = new("/Pages/Shared/_Message.cshtml", "Message");
```

Use them for compile-time safety instead of magic strings:

```csharp
await builder.AppendAsync("messages", Partials.Message, message);
```

## Configuration

```csharp
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";
    options.AddVaryHeader = true;
});
```

## Helper Extensions

Check if a request is a Turbo Frame request:

```csharp
if (HttpContext.IsTurboFrameRequest())
{
    return Partial("_MyPartial", Model);
}

// Or check for a specific frame
if (HttpContext.IsTurboFrameRequest("cart-items"))
{
    return Partial("_CartItems", Model);
}

// Or check for a prefix (dynamic IDs)
if (HttpContext.IsTurboFrameRequestWithPrefix("item_"))
{
    return Partial("_CartItem", Model);
}

// Get the raw frame ID
string? frameId = HttpContext.GetTurboFrameId();
```

## How It Works

1. User clicks a link or submits a form targeting a `<turbo-frame>`
2. Turbo.js sends a request with the `Turbo-Frame` header
3. Your page handler checks for the header and returns a partial view
4. Turbo.js extracts the matching frame from the response and updates the DOM

This approach is simple, explicit, and gives you full control over what content is returned.

## Documentation

### Guides
- [Turbo Frames Guide](docs/guides/turbo-frames.md) - Partial page updates
- [Turbo Streams Guide](docs/guides/turbo-streams.md) - Real-time updates
- [Authorization Guide](docs/guides/authorization.md) - Securing streams
- [Testing Guide](docs/guides/testing.md) - Testing strategies
- [Troubleshooting](docs/guides/troubleshooting.md) - Common issues

### API Reference
- [ITurbo](docs/api/ITurbo.md) - Main service interface
- [ITurboStreamBuilder](docs/api/ITurboStreamBuilder.md) - Stream builder
- [TurboOptions](docs/api/TurboOptions.md) - Configuration
- [Tag Helpers](docs/api/TagHelpers.md) - Razor tag helpers

### Migration Guides
- [From Blazor Server](docs/migration/from-blazor-server.md)
- [From HTMX](docs/migration/from-htmx.md)

## Sample Applications

The repository includes three sample applications:

**[Tombatron.Turbo.Sample](samples/Tombatron.Turbo.Sample)** - Turbo Frames for partial page updates and Turbo Streams for real-time notifications, including a shopping cart with add/remove operations.

**[Tombatron.Turbo.Chat](samples/Tombatron.Turbo.Chat)** - Real-time multi-room chat using room-based streams, typing indicators, private messaging, and the source-generated `Partials` class for strongly-typed partial rendering.

**[Tombatron.Turbo.Dashboard](samples/Tombatron.Turbo.Dashboard)** - Live metrics dashboard using `Broadcast()` from a background service to push updates to all connected clients every 2 seconds.

Run any sample:

```bash
cd samples/Tombatron.Turbo.Sample
dotnet run
```

## Requirements

- .NET 10.0 or later
- ASP.NET Core
- Turbo.js 8.x (client-side)
- SignalR (for Turbo Streams)

## Publishing / Releases

Both the NuGet and npm packages are published automatically when a version tag is pushed:

```bash
git tag v1.2.3
git push origin v1.2.3
```

This triggers the [Release workflow](.github/workflows/release.yml) which builds, tests, and publishes:
- **Tombatron.Turbo** to [NuGet](https://www.nuget.org/packages/Tombatron.Turbo/)
- **@tombatron/turbo-signalr** to [npm](https://www.npmjs.com/package/@tombatron/turbo-signalr)

The npm package can also be published independently via the [manual workflow](.github/workflows/npm-publish.yml).

## License

MIT License - see [LICENSE](LICENSE) for details.
