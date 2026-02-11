# ITurboStreamBuilder Interface

Builder interface for constructing Turbo Stream actions. Provides a fluent API for building DOM updates that are sent to clients.

## Namespace

```csharp
Tombatron.Turbo
```

## Methods

All methods return the builder instance for method chaining.

### Append(string target, string html)

Appends content to the **end** of the target element's children.

```csharp
builder.Append("notifications", "<div class='notification'>New message</div>");
```

**Generated HTML:**
```html
<turbo-stream action="append" target="notifications">
  <template><div class='notification'>New message</div></template>
</turbo-stream>
```

### Prepend(string target, string html)

Prepends content to the **beginning** of the target element's children.

```csharp
builder.Prepend("activity-feed", "<div class='activity'>User logged in</div>");
```

### Replace(string target, string html)

Replaces the **entire** target element with the provided content.

```csharp
builder.Replace("user-card", "<div id='user-card'>Updated Card</div>");
```

> **Important:** When using `Replace`, include the target element's ID in the replacement HTML if you want to target it again later.

### Update(string target, string html)

Updates the **inner content** of the target element without replacing the element itself.

```csharp
builder.Update("cart-total", "$149.99");
```

This is the most commonly used action for updating text or simple content.

### Remove(string target)

Removes the target element from the DOM entirely.

```csharp
builder.Remove("item-123");
```

### Before(string target, string html)

Inserts content immediately **before** the target element (as a sibling).

```csharp
builder.Before("add-item-button", "<div class='item'>New Item</div>");
```

### After(string target, string html)

Inserts content immediately **after** the target element (as a sibling).

```csharp
builder.After("item-5", "<div class='item'>Item 6</div>");
```

### Build()

Builds the final Turbo Stream HTML containing all configured actions. This is called internally by `ITurbo.Stream()` and typically doesn't need to be called directly.

```csharp
string html = builder.Build();
```

## Method Chaining

All action methods can be chained together:

```csharp
await _turbo.Stream("user:123", builder =>
{
    builder
        .Append("notifications", "<div>Order confirmed!</div>")
        .Update("cart-count", "0")
        .Remove("checkout-button")
        .Replace("cart-summary", "<div id='cart-summary'>Cart is empty</div>");
});
```

## Action Summary

| Action | Target Element | Content |
|--------|---------------|---------|
| `Append` | Preserved | Added as last child |
| `Prepend` | Preserved | Added as first child |
| `Replace` | Removed | Replaces entire element |
| `Update` | Preserved | Replaces inner content |
| `Remove` | Removed | N/A |
| `Before` | Preserved | Added before element |
| `After` | Preserved | Added after element |

## Validation

All methods validate their parameters:

- `target` cannot be null, empty, or whitespace
- `html` cannot be null (except for `Remove`)

Invalid parameters throw `ArgumentNullException` or `ArgumentException`.

## See Also

- [ITurbo](ITurbo.md) - Main service interface
- [Turbo Streams Guide](../guides/turbo-streams.md) - Complete guide
