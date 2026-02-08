# Tombatron.Turbo

[![Build and Test](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml/badge.svg)](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Tombatron.Turbo.svg)](https://www.nuget.org/packages/Tombatron.Turbo/)

Hotwire Turbo for ASP.NET Core with compile-time frame optimization and SignalR-powered real-time streams.

## Features

- **Turbo Frames**: Partial page updates with compile-time optimization
- **Turbo Streams**: Real-time updates via SignalR
- **Source Generator**: Generates optimized sub-templates at compile time
- **Roslyn Analyzer**: Enforces frame ID rules at compile time
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
app.UseEndpoints(endpoints => { ... });
```

### 3. Add Turbo Frames to Your Views

```html
<!-- Static frame ID - automatically optimized at compile time -->
<turbo-frame id="cart-items">
    @foreach (var item in Model.Items)
    {
        <div>@item.Name</div>
    }
</turbo-frame>

<!-- Dynamic frame ID - requires prefix for optimization -->
<turbo-frame id="item_@item.Id" asp-frame-prefix="item_">
    <div>@item.Name</div>
</turbo-frame>
```

### 4. Broadcast Real-Time Updates

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

## Core Rule

**Static frame IDs only, unless you provide a prefix. Dynamic IDs without prefix = compile error.**

```html
<!-- ✅ Valid: Static ID -->
<turbo-frame id="cart-items">...</turbo-frame>

<!-- ✅ Valid: Dynamic ID with prefix -->
<turbo-frame id="item_@item.Id" asp-frame-prefix="item_">...</turbo-frame>

<!-- ❌ Compile Error: Dynamic ID without prefix -->
<turbo-frame id="@item.Id">...</turbo-frame>
```

## Configuration

```csharp
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";
    options.UseSignedStreamNames = true;
    options.DefaultUserStreamPattern = "user:{0}";
    options.DefaultSessionStreamPattern = "session:{0}";
});
```

## Documentation

- [Turbo Frames Guide](docs/guides/turbo-frames.md)
- [Turbo Streams Guide](docs/guides/turbo-streams.md)
- [Authorization Guide](docs/guides/authorization.md)
- [API Reference](docs/api/)

## License

MIT License - see [LICENSE](LICENSE) for details.
