---
title: Import Maps
sidebar_label: Import Maps
sidebar_position: 4
description: Pin JavaScript modules with import maps for Turbo.js and custom libraries.
---

Pin additional modules in `Program.cs`:

```csharp
builder.Services.AddTurbo(options =>
{
    options.ImportMap.Pin("my-lib", "/js/my-lib.js", preload: true);
    options.ImportMap.Unpin("turbo-signalr"); // Remove a default pin
});
```

## Default Pins

These are set automatically when you call `AddTurbo()`:

- `@hotwired/turbo` — Turbo.js 8.x from unpkg (preloaded)
- `turbo-signalr` — Bundled SignalR bridge from NuGet (preloaded)

When using `AddStimulus()`, the Stimulus library and a generated controller index are also pinned automatically.

## Rendering

Add the tag helper to your layout:

```html
<turbo-scripts mode="Importmap" />
```

This renders an `<script type="importmap">` block followed by `<link rel="modulepreload">` tags for preloaded modules.

For traditional `<script>` tags instead:

```html
<turbo-scripts />
```

## Custom Modules

Pin your own JavaScript modules to make them available via `import`:

```csharp
options.ImportMap.Pin("chart-lib", "https://cdn.example.com/chart.js");
options.ImportMap.Pin("app", "/js/app.js", preload: true);
```

Then in your JavaScript:

```javascript
import Chart from "chart-lib";
import { init } from "app";
```
