# Tombatron.Turbo

[![Build and Test](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml/badge.svg)](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Tombatron.Turbo.svg)](https://www.nuget.org/packages/Tombatron.Turbo/)
[![npm](https://img.shields.io/npm/v/@tombatron/turbo-signalr.svg)](https://www.npmjs.com/package/@tombatron/turbo-signalr)

Hotwire Turbo for ASP.NET Core with SignalR-powered real-time streams.

## Features

- **Turbo Frames**: Partial page updates using manual partials
- **Turbo Streams**: Real-time updates via SignalR
- **Simple Architecture**: Check for `Turbo-Frame` header, return partial or redirect
- **Zero JavaScript Configuration**: Works out of the box with Turbo.js

## Installation

**NuGet (ASP.NET Core server package):**

```bash
dotnet add package Tombatron.Turbo
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

### Broadcast Updates from Server

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

        // Broadcast update to the user's stream
        await _turbo.Stream($"user:{User.Identity.Name}", builder =>
        {
            builder.Update("cart-total", $"<span>${cart.Total}</span>");
        });

        return Ok();
    }
}
```

### Include the Client Script

```html
<!-- In your layout: Turbo.js + SignalR adapter -->
<script type="module" src="https://cdn.jsdelivr.net/npm/@hotwired/turbo@8/dist/turbo.es2017-esm.min.js"></script>
<script src="_content/Tombatron.Turbo/dist/turbo-signalr.bundled.min.js"></script>
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

## Configuration

```csharp
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";
    options.UseSignedStreamNames = true;
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

## Sample Application

The repository includes a sample application demonstrating:
- Turbo Frames for partial updates
- Turbo Streams for real-time features
- Shopping cart with add/remove operations
- Live notifications and counters

Run the sample:

```bash
cd samples/Tombatron.Turbo.Sample
dotnet run
```

## Requirements

- .NET 9.0 or later
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
