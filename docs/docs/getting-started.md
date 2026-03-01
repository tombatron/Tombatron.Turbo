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

app.UseStaticFiles();
app.UseRouting();
app.UseTurbo();

app.MapRazorPages();
app.MapTurboHub();

app.Run();
```

`AddTurbo()` registers the Turbo services and tag helpers. `UseTurbo()` adds middleware that sets the `Vary` header on Turbo Frame responses. `MapTurboHub()` exposes the SignalR hub for Turbo Streams.

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

This renders Turbo.js, the SignalR bridge, and (optionally) Stimulus via an import map.

## 5. Create your first Turbo Frame

Wrap a section of your page in a `<turbo-frame>`:

```html
<turbo-frame id="greeting">
    <p>Hello, world!</p>
    <a href="/greeting?handler=Refresh" data-turbo-frame="greeting">
        Refresh
    </a>
</turbo-frame>
```

In your page model, return a partial for Turbo Frame requests:

```csharp
using Tombatron.Turbo;

public IActionResult OnGetRefresh()
{
    if (HttpContext.IsTurboFrameRequest())
    {
        return Partial("_Greeting", Model);
    }

    return RedirectToPage();
}
```

That's it — clicking "Refresh" updates only the frame, not the whole page.

## Next steps

- [Build a Todo List](tutorials/todo-list.md) — full walkthrough with forms, validation, and Stimulus
- [Real-Time Streams](tutorials/real-time-streams.md) — push live updates with SignalR
- [Turbo Frames Guide](guides/turbo-frames.md) — deep dive into frames
- [API Reference](api/ITurbo.md) — full API docs
