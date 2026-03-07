---
title: Getting Started
sidebar_label: Getting Started
sidebar_position: 1
description: Install Tombatron.Turbo and build your first Turbo Frame in under five minutes.
---

Get up and running with Tombatron.Turbo in three steps.

## 1. Install the package

```bash
dotnet add package Tombatron.Turbo
```

## 2. Configure services

In `Program.cs`:

```csharp
using Tombatron.Turbo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTurbo();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseRouting();
app.UseTurbo();

app.MapRazorPages();
app.MapTurboHub();
app.MapStaticAssets();

app.Run();
```

`AddTurbo()` registers the Turbo services that are required to communicate with the Turbo front-end as well as the tag helpers we've defined on the back-end. `UseTurbo()` adds middleware that sets the `Vary` header on Turbo Frame responses. `MapTurboHub()` exposes the SignalR hub for Turbo Streams.

## 3. Register tag helpers

Add to `Pages/_ViewImports.cshtml`:

```razor
@addTagHelper *, Tombatron.Turbo
```

## 4. Add Turbo scripts to your layout

In your layout file (e.g. `Pages/Shared/_Layout.cshtml`), add the script tag helper:

```html
<head>
    <!-- ... -->
    <turbo-scripts mode="Importmap" />
</head>
```

This renders Turbo.js, the SignalR bridge, and (optionally) Stimulus via an [import map](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/script/type/importmap).

> **Note:** Turbo.js must be loaded before the SignalR adapter. If Turbo.js is missing, stream messages will be dropped and a warning will appear in the browser console.

We recommend **Importmap** mode because it uses native browser module resolution — no bundler, no build step. The tag helper emits a `<script type="importmap">` block that maps bare module specifiers (like `"@hotwired/turbo"`) to URLs, then loads them with standard `<script type="module">` imports. The browser handles dependency resolution natively, which means:

- **No build tooling required** — no Webpack, Vite, or esbuild to configure
- **Fine-grained caching** — each module is a separate cacheable resource; updating one doesn't invalidate the rest
- **Transparent dependency graph** — you can inspect the import map in view-source to see exactly what's loaded

A `Traditional` mode is also available if you prefer classic `<script>` tags. Set `mode="Traditional"` to emit individual script elements instead.

## 5. Create your first Turbo Frame

First, create a partial view at `Pages/Shared/_Greeting.cshtml`:

```html
@model IndexModel

<turbo-frame id="greeting">
    <p>@Model.Greeting</p>
    <a href="/?handler=Refresh" data-turbo-frame="greeting">
        Refresh
    </a>
</turbo-frame>
```

Then render it from your page. In `Pages/Index.cshtml`:

```html
@page
@model IndexModel

<partial name="_Greeting" model="Model" />
```

In your page model (`Pages/Index.cshtml.cs`), return the partial for Turbo Frame requests. Use the source-generated `Partials` class for compile-time safety instead of passing partial names as strings:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tombatron.Turbo;
using Tombatron.Turbo.Generated;

public class IndexModel : PageModel
{
    private static readonly string[] Greetings =
    [
        "Hello, world!",
        "Howdy, partner!",
        "Bonjour, le monde!",
        "Hola, mundo!",
        "Ciao, mondo!",
        "Hej, världen!"
    ];

    public string Greeting { get; set; } = Greetings[0];

    public void OnGet() { }

    public IActionResult OnGetRefresh()
    {
        Greeting = Greetings[Random.Shared.Next(Greetings.Length)];

        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial(Partials.Greeting.ViewPath, this);
        }

        return RedirectToPage();
    }
}
```

The `Partials` class is [generated at compile time](reference/source-generator.md) from your `_*.cshtml` files — typos in partial names become build errors instead of runtime failures.

That's it — clicking "Refresh" updates only the frame, not the whole page.

## Next steps

- [Build a Todo List](tutorials/todo-list.md) — full walkthrough with forms, validation, and Stimulus
- [Real-Time Streams](tutorials/real-time-streams.md) — push live updates with SignalR
- [Turbo Frames Guide](guides/turbo-frames.md) — deep dive into frames
- [API Reference](api/ITurbo.md) — full API docs
