---
title: Source Generator
sidebar_label: Source Generator
sidebar_position: 6
description: Compile-time strongly-typed partial references with the bundled source generator.
---

The source generator is bundled with `Tombatron.Turbo` — no extra package needed. It scans `_*.cshtml` partial views at compile time and generates a `Partials` class (in `Tombatron.Turbo.Generated`) with strongly-typed references.

## Usage

```csharp
// Use generated references instead of string names:
await builder.AppendAsync("messages", Partials.Message, message);
```

## How It Works

1. At compile time, the generator finds all `_*.cshtml` files in your project
2. It generates a static `Partials` class with a `PartialTemplate` property for each partial
3. These references can be used with `ITurboStreamBuilder` and `TurboResults` methods

## Benefits

- **Compile-time safety** — Typos in partial names are caught at build time, not runtime
- **Refactoring support** — Rename a partial and the compiler flags all usages
- **IntelliSense** — Autocomplete for partial names in your IDE

## Example

Given a partial `Pages/Shared/_CartItem.cshtml`, the generator creates:

```csharp
// Auto-generated
public static class Partials
{
    public static PartialTemplate CartItem { get; }
}
```

Use it in stream builders:

```csharp
await _turbo.Stream($"user:{userId}", async builder =>
{
    await builder.AppendAsync("cart-items", Partials.CartItem, newItem);
});
```

Or in Minimal API endpoints:

```csharp
return TurboResults.Partial(Partials.CartItem, item);
```
