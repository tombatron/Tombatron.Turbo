# Turbo Frames Guide

Turbo Frames enable partial page updates by wrapping sections of your page in `<turbo-frame>` elements. When a link or form within a frame is activated, only that frame's content is updated.

## How It Works

1. User clicks a link or submits a form inside a `<turbo-frame>`
2. Turbo.js sends a request with the `Turbo-Frame` header
3. Your server checks for this header and returns a partial view
4. Turbo.js extracts the matching frame from the response and updates the DOM

## Basic Setup

### 1. Create a Partial View

```html
<!-- Pages/Shared/_CartItems.cshtml -->
<turbo-frame id="cart-items">
    @foreach (var item in Model.Items)
    {
        <div class="cart-item">
            <span>@item.Name</span>
            <span>@item.Price.ToString("C")</span>
        </div>
    }
</turbo-frame>
```

### 2. Include the Partial in Your Page

```html
<!-- Pages/Cart/Index.cshtml -->
@page
@model CartModel

<h1>Shopping Cart</h1>

<partial name="_CartItems" model="Model" />

<a href="/Cart?handler=Refresh" data-turbo-frame="cart-items">
    Refresh Cart
</a>
```

### 3. Handle Frame Requests

```csharp
// Pages/Cart/Index.cshtml.cs
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

        // Check if this is a Turbo Frame request
        if (Request.Headers.ContainsKey("Turbo-Frame"))
        {
            return Partial("_CartItems", this);
        }

        // Regular request - redirect to full page
        return RedirectToPage();
    }
}
```

## Helper Extensions

Use the helper extensions for cleaner code:

```csharp
using Tombatron.Turbo;

public IActionResult OnGetRefresh()
{
    Items = GetCartItems();

    if (HttpContext.IsTurboFrameRequest())
    {
        return Partial("_CartItems", this);
    }

    return RedirectToPage();
}
```

### Available Extensions

```csharp
// Check if any Turbo Frame request
HttpContext.IsTurboFrameRequest()

// Check for specific frame
HttpContext.IsTurboFrameRequest("cart-items")

// Check for frames with a prefix (dynamic IDs)
HttpContext.IsTurboFrameRequestWithPrefix("item_")

// Get the frame ID
string? frameId = HttpContext.GetTurboFrameId();
```

## Dynamic Frame IDs

For lists of items, use dynamic IDs with a consistent prefix:

```html
<!-- Partial for each item -->
@foreach (var item in Model.Items)
{
    <turbo-frame id="item_@item.Id">
        <div class="item">
            <span>@item.Name</span>
            <form method="post" asp-page-handler="Remove" asp-route-id="@item.Id">
                <button type="submit">Remove</button>
            </form>
        </div>
    </turbo-frame>
}
```

Handle with prefix matching:

```csharp
public IActionResult OnPostRemove(int id)
{
    RemoveItem(id);

    if (HttpContext.IsTurboFrameRequestWithPrefix("item_"))
    {
        // Return empty frame to remove the item
        return Content($"<turbo-frame id=\"item_{id}\"></turbo-frame>", "text/html");
    }

    return RedirectToPage();
}
```

## Lazy Loading

Load content only when the frame scrolls into view:

```html
<turbo-frame id="comments" src="/posts/@Model.Id/comments" loading="lazy">
    <div class="loading-placeholder">
        Loading comments...
    </div>
</turbo-frame>
```

The frame will request `/posts/{id}/comments` when it becomes visible.

## Navigation Patterns

### Breaking Out of Frames

Use `target="_top"` to make links navigate the whole page:

```html
<turbo-frame id="search-results">
    @foreach (var result in Model.Results)
    {
        <!-- This link navigates the whole page, not just the frame -->
        <a href="/products/@result.Id" data-turbo-frame="_top">
            @result.Name
        </a>
    }
</turbo-frame>
```

Or set it on the frame itself:

```html
<turbo-frame id="login" target="_top">
    <!-- All links/forms in this frame will navigate the full page -->
</turbo-frame>
```

### Targeting Other Frames

A link can update a different frame:

```html
<turbo-frame id="sidebar">
    <a href="/products/1" data-turbo-frame="main-content">View Product</a>
</turbo-frame>

<turbo-frame id="main-content">
    <!-- Product details appear here -->
</turbo-frame>
```

### Disabling Frame Navigation

Prevent the frame from intercepting navigation:

```html
<turbo-frame id="preview" disabled>
    <!-- Links and forms work normally -->
</turbo-frame>
```

## Best Practices

### 1. Always Return a Matching Frame

The response must contain a `<turbo-frame>` with the same ID:

```html
<!-- Request: Turbo-Frame: cart-items -->
<!-- Response must include: -->
<turbo-frame id="cart-items">
    <!-- Updated content -->
</turbo-frame>
```

### 2. Handle Non-Turbo Requests

Always provide a fallback for regular requests:

```csharp
if (HttpContext.IsTurboFrameRequest())
{
    return Partial("_MyPartial", Model);
}
return RedirectToPage();
```

### 3. Use Semantic IDs

Choose IDs that describe the content:

```html
<!-- Good -->
<turbo-frame id="user-profile">
<turbo-frame id="order-history">
<turbo-frame id="product-reviews">

<!-- Avoid -->
<turbo-frame id="frame1">
<turbo-frame id="content">
```

### 4. Keep Frames Focused

Each frame should represent a single, cohesive piece of content. If you need to update multiple areas, consider using [Turbo Streams](turbo-streams.md) instead.

## Troubleshooting

### Frame Not Updating

1. Check that response contains a frame with matching ID
2. Verify the `Turbo-Frame` header is present in the request
3. Ensure partial view renders the complete frame element

### Full Page Loads Instead of Frame Updates

1. Check that Turbo.js is loaded on the page
2. Verify the link/form is inside the frame or uses `data-turbo-frame`
3. Ensure `target="_top"` isn't set unintentionally

### Caching Issues

The `Vary: Turbo-Frame` header is added automatically to prevent caching issues. If using a CDN, ensure it respects this header.

## See Also

- [Tag Helpers](../api/TagHelpers.md) - Frame tag helper reference
- [Turbo Streams](turbo-streams.md) - Real-time updates
- [Troubleshooting](troubleshooting.md) - Common issues
