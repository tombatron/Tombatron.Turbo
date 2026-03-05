---
title: ITurboStreamBuilder Interface
sidebar_label: ITurboStreamBuilder
sidebar_position: 2
description: Builder interface for constructing Turbo Stream actions with a fluent API for building DOM updates.
---

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

### Replace(string target, string html, bool morph = false)

Replaces the **entire** target element with the provided content.

```csharp
builder.Replace("user-card", "<div id='user-card'>Updated Card</div>");
```

When `morph` is `true`, Turbo uses DOM morphing (via idiomorph) to update the element, preserving DOM state such as form inputs, focus, and scroll positions:

```csharp
builder.Replace("user-card", "<div id='user-card'>Updated Card</div>", morph: true);
```

**Generated HTML with morph:**
```html
<turbo-stream action="replace" method="morph" target="user-card">
  <template><div id='user-card'>Updated Card</div></template>
</turbo-stream>
```

> **Important:** When using `Replace`, include the target element's ID in the replacement HTML if you want to target it again later.

### Update(string target, string html, bool morph = false)

Updates the **inner content** of the target element without replacing the element itself.

```csharp
builder.Update("cart-total", "$149.99");
```

When `morph` is `true`, Turbo uses DOM morphing to intelligently diff and patch the inner content:

```csharp
builder.Update("product-list", newListHtml, morph: true);
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

### Refresh(string? requestId = null, bool morph = false, bool preserveScroll = false)

Tells clients to perform a page refresh. Optionally includes a request ID so the originating client can suppress the redundant refresh.

**Parameters:**
- `requestId` — The `X-Turbo-Request-Id` of the originating request, or `null` for no suppression
- `morph` — When `true`, uses morphing for the refresh instead of full page replacement
- `preserveScroll` — When `true`, preserves scroll position during the refresh

```csharp
builder.Refresh();
builder.Refresh(requestId: "abc-123", morph: true, preserveScroll: true);
```

**Generated HTML:**
```html
<turbo-stream action="refresh"></turbo-stream>
<turbo-stream action="refresh" method="morph" scroll="preserve" request-id="abc-123"></turbo-stream>
```

> **Note:** The `Refresh` action does not use a `<template>` or target — it triggers a full page re-fetch. Use `morph` and `preserveScroll` to make refreshes less disruptive. For convenience, see also `ITurbo.StreamRefresh()` and `ITurbo.BroadcastRefresh()`.

## CSS Selector Targeting

The `*All` methods work like their single-target counterparts but use a CSS selector via the `targets` attribute (plural) instead of a DOM ID via `target`. This lets you update multiple elements in one action.

### AppendAll(string targets, string html)

Appends content to the end of **all** elements matching the CSS selector.

```csharp
builder.AppendAll(".notification-list", "<div class='notification'>New alert</div>");
```

**Generated HTML:**
```html
<turbo-stream action="append" targets=".notification-list">
  <template><div class='notification'>New alert</div></template>
</turbo-stream>
```

### PrependAll(string targets, string html)

Prepends content to the beginning of **all** elements matching the CSS selector.

```csharp
builder.PrependAll(".feed", "<div class='entry'>Latest event</div>");
```

### ReplaceAll(string targets, string html, bool morph = false)

Replaces **all** elements matching the CSS selector with the provided content.

```csharp
builder.ReplaceAll(".stale-card", "<div class='card'>Refreshed</div>");
builder.ReplaceAll(".stale-card", "<div class='card'>Refreshed</div>", morph: true);
```

### UpdateAll(string targets, string html, bool morph = false)

Updates the inner content of **all** elements matching the CSS selector.

```csharp
builder.UpdateAll(".price", "$9.99");
builder.UpdateAll(".price", "$9.99", morph: true);
```

### RemoveAll(string targets)

Removes **all** elements matching the CSS selector from the DOM.

```csharp
builder.RemoveAll(".dismissed");
```

**Generated HTML:**
```html
<turbo-stream action="remove" targets=".dismissed"></turbo-stream>
```

### BeforeAll(string targets, string html)

Inserts content immediately **before** all elements matching the CSS selector.

```csharp
builder.BeforeAll(".section-header", "<hr class='divider'>");
```

### AfterAll(string targets, string html)

Inserts content immediately **after** all elements matching the CSS selector.

```csharp
builder.AfterAll(".item", "<div class='separator'></div>");
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

| Action | Target Element | Content | Morph Support |
|--------|---------------|---------|---------------|
| `Append` | Preserved | Added as last child | No |
| `Prepend` | Preserved | Added as first child | No |
| `Replace` | Removed | Replaces entire element | Yes |
| `Update` | Preserved | Replaces inner content | Yes |
| `Remove` | Removed | N/A | No |
| `Before` | Preserved | Added before element | No |
| `After` | Preserved | Added after element | No |
| `Refresh` | N/A (full page) | N/A | Yes |

Each action above (except `Refresh`) has a corresponding `*All` variant that accepts a CSS selector via the `targets` parameter instead of a single DOM ID:

| Single Target | CSS Selector |
|---------------|-------------|
| `Append(target, html)` | `AppendAll(targets, html)` |
| `Prepend(target, html)` | `PrependAll(targets, html)` |
| `Replace(target, html, morph)` | `ReplaceAll(targets, html, morph)` |
| `Update(target, html, morph)` | `UpdateAll(targets, html, morph)` |
| `Remove(target)` | `RemoveAll(targets)` |
| `Before(target, html)` | `BeforeAll(targets, html)` |
| `After(target, html)` | `AfterAll(targets, html)` |

## Validation

All methods validate their parameters:

- `target` cannot be null, empty, or whitespace
- `targets` (CSS selector) cannot be null, empty, or whitespace
- `html` cannot be null (except for `Remove`/`RemoveAll`)

Invalid parameters throw `ArgumentNullException` or `ArgumentException`.

## See Also

- [ITurbo](ITurbo.md) - Main service interface
- [Turbo Streams Guide](../guides/turbo-streams.md) - Complete guide
