# Tombatron.Turbo

[![Build and Test](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml/badge.svg)](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Tombatron.Turbo.svg)](https://www.nuget.org/packages/Tombatron.Turbo/)

Hotwire Turbo for ASP.NET Core with SignalR-powered real-time streams.

## Features

- **Turbo Frames**: Partial page updates using manual partials
- **Turbo Streams**: Real-time updates via SignalR
- **Simple Architecture**: Check for `Turbo-Frame` header, return partial or redirect
- **Zero JavaScript Configuration**: Works out of the box with Turbo.js

## Installation

```bash
dotnet add package Tombatron.Turbo
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

### 3. Create a Turbo Frame with a Partial

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

### 4. Handle Frame Requests in Your Page Model

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
        if (Request.Headers.ContainsKey("Turbo-Frame"))
        {
            return Partial("_CartItems", this);
        }

        // For regular requests, redirect to the full page
        return RedirectToPage();
    }
}
```

### 5. Link to the Handler

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

### Subscribe to Streams in Your View

```html
<turbo-stream-source-signalr stream="user:@User.Identity.Name" hub-url="/turbo-hub">
</turbo-stream-source-signalr>

<div id="cart-total">$0.00</div>
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

## License

MIT License - see [LICENSE](LICENSE) for details.
