# Tombatron.Turbo

[![Build and Test](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml/badge.svg)](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Tombatron.Turbo.svg)](https://www.nuget.org/packages/Tombatron.Turbo/)
[![npm](https://img.shields.io/npm/v/@tombatron/turbo-signalr.svg)](https://www.npmjs.com/package/@tombatron/turbo-signalr)

Hotwire Turbo for ASP.NET Core with SignalR-powered real-time streams.

**[Read the full documentation](https://tombatron.github.io/Tombatron.Turbo/)**

## Features

- **Turbo Frames** — Partial page updates with automatic `Turbo-Frame` header detection
- **Turbo Streams** — Real-time updates via SignalR with targeted and broadcast support
- **Stimulus** — Convention-based controller discovery with import maps and hot reload
- **Source Generator** — Compile-time strongly-typed partial references
- **Form Validation** — HTTP 422 support for inline validation errors within Turbo Frames
- **Minimal API Support** — Return partials from Minimal API endpoints with `TurboResults`
- **Import Maps** — Pin JavaScript modules with `<turbo-scripts mode="Importmap" />`
- **Zero Configuration** — Works out of the box with Turbo.js

## Quick Start

```bash
dotnet add package Tombatron.Turbo
```

```csharp
// Program.cs
builder.Services.AddTurbo();
app.UseTurbo();
app.MapTurboHub();
```

```razor
@* _ViewImports.cshtml *@
@addTagHelper *, Tombatron.Turbo
```

```html
<!-- Layout -->
<turbo-scripts mode="Importmap" />
```

Then wrap a section in a Turbo Frame:

```html
<turbo-frame id="cart" src="/cart" loading="lazy">
    Loading...
</turbo-frame>
```

For the full walkthrough, see the [Getting Started guide](https://tombatron.github.io/Tombatron.Turbo/docs/getting-started).

## Sample Applications

**[Tombatron.Turbo.Sample](samples/Tombatron.Turbo.Sample)** — Turbo Frames, Turbo Streams, shopping cart, and form validation demo.

```bash
cd samples/Tombatron.Turbo.Sample
dotnet run
```

## Requirements

- .NET 10.0 or later
- ASP.NET Core
- Turbo.js 8.x (included via tag helper)
- SignalR (for Turbo Streams)

## Publishing / Releases

Both the NuGet and npm packages are published automatically when a version tag is pushed:

```bash
git tag v1.2.3
git push origin v1.2.3
```

This triggers the [Release workflow](.github/workflows/release.yml) which publishes **Tombatron.Turbo** to [NuGet](https://www.nuget.org/packages/Tombatron.Turbo/) and **@tombatron/turbo-signalr** to [npm](https://www.npmjs.com/package/@tombatron/turbo-signalr).

## License

MIT License - see [LICENSE](LICENSE) for details.
