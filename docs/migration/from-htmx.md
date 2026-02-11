# Migrating from HTMX

This guide helps you migrate from HTMX to Tombatron.Turbo. Both libraries share similar philosophies, making migration straightforward.

## Overview

| HTMX | Tombatron.Turbo |
|------|-----------------|
| `hx-*` attributes | Turbo Frames + `data-turbo-*` |
| Any HTTP verb | Standard forms + frames |
| Custom swap strategies | Seven built-in actions |
| Server-Sent Events | SignalR WebSocket |
| 14KB gzipped | ~10KB (Turbo) + ~30KB (SignalR) |

## Attribute Mapping

### Basic Requests

**HTMX:**
```html
<button hx-get="/api/data" hx-target="#result">Load</button>
<div id="result"></div>
```

**Turbo:**
```html
<turbo-frame id="result">
    <a href="/api/data">Load</a>
</turbo-frame>
```

Or for buttons:
```html
<turbo-frame id="result">
    <form method="get" action="/api/data">
        <button type="submit">Load</button>
    </form>
</turbo-frame>
```

### POST Requests

**HTMX:**
```html
<form hx-post="/api/create" hx-target="#list" hx-swap="beforeend">
    <input name="item" />
    <button>Add</button>
</form>
```

**Turbo (using Streams for append):**
```html
<form method="post" action="/api/create">
    <input name="item" />
    <button>Add</button>
</form>
```

```csharp
public IActionResult OnPost(string item)
{
    // Create item...

    if (Request.Headers.Accept.ToString().Contains("text/vnd.turbo-stream.html"))
    {
        return Content($@"
            <turbo-stream action=""append"" target=""list"">
                <template><div>{item}</div></template>
            </turbo-stream>
        ", "text/vnd.turbo-stream.html");
    }

    return RedirectToPage();
}
```

### Swap Strategies

| HTMX `hx-swap` | Turbo Stream Action |
|----------------|---------------------|
| `innerHTML` | `update` |
| `outerHTML` | `replace` |
| `beforeend` | `append` |
| `afterbegin` | `prepend` |
| `beforebegin` | `before` |
| `afterend` | `after` |
| `delete` | `remove` |

### Triggers

**HTMX:**
```html
<input hx-get="/search" hx-trigger="keyup changed delay:500ms" hx-target="#results" />
```

**Turbo:** Use JavaScript or a Stimulus controller:
```html
<turbo-frame id="results">
    <input data-controller="search" data-action="input->search#submit" />
</turbo-frame>
```

```javascript
// Using Stimulus
import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
    submit = debounce(() => {
        this.element.closest('form').requestSubmit()
    }, 500)
}
```

### Loading Indicators

**HTMX:**
```html
<button hx-get="/data" hx-indicator="#spinner">Load</button>
<div id="spinner" class="htmx-indicator">Loading...</div>
```

**Turbo:** Use CSS with Turbo's built-in classes:
```html
<style>
    .turbo-progress-bar { display: block; }
    turbo-frame[busy] .loading { display: block; }
    turbo-frame:not([busy]) .loading { display: none; }
</style>

<turbo-frame id="content">
    <a href="/data">Load</a>
    <div class="loading">Loading...</div>
</turbo-frame>
```

## Real-Time Updates

### Server-Sent Events (HTMX) to SignalR (Turbo)

**HTMX:**
```html
<div hx-sse="connect:/events">
    <div hx-sse="swap:message" id="notifications"></div>
</div>
```

**Turbo:**
```html
<turbo stream="notifications"></turbo>
<div id="notifications"></div>
```

```csharp
// Server
await _turbo.Stream("notifications", b =>
    b.Append("notifications", "<div>New message</div>"));
```

## Common Patterns

### Inline Editing

**HTMX:**
```html
<div hx-get="/item/1/edit" hx-trigger="click" hx-swap="outerHTML">
    Click to edit
</div>
```

**Turbo:**
```html
<turbo-frame id="item-1">
    <a href="/item/1/edit">Click to edit</a>
</turbo-frame>
```

### Infinite Scroll

**HTMX:**
```html
<div hx-get="/items?page=2" hx-trigger="revealed" hx-swap="afterend">
    Loading more...
</div>
```

**Turbo:**
```html
<turbo-frame id="items-page-2" src="/items?page=2" loading="lazy">
    Loading more...
</turbo-frame>
```

### Delete Confirmation

**HTMX:**
```html
<button hx-delete="/item/1" hx-confirm="Are you sure?" hx-target="#item-1" hx-swap="delete">
    Delete
</button>
```

**Turbo:**
```html
<form method="post" action="/item/1/delete"
      data-turbo-confirm="Are you sure?">
    <button type="submit">Delete</button>
</form>
```

### Out-of-Band Updates

**HTMX:**
```html
<!-- Response -->
<div id="main">Main content</div>
<div id="sidebar" hx-swap-oob="true">Updated sidebar</div>
```

**Turbo (using Streams):**
```html
<turbo-stream action="replace" target="main">
    <template>Main content</template>
</turbo-stream>
<turbo-stream action="replace" target="sidebar">
    <template>Updated sidebar</template>
</turbo-stream>
```

## Key Differences

### 1. Navigation Model

HTMX works with any element. Turbo Frames require wrapping content in `<turbo-frame>` elements.

### 2. HTTP Methods

HTMX supports `hx-get`, `hx-post`, `hx-put`, `hx-patch`, `hx-delete` on any element. Turbo uses standard forms and links.

### 3. Progressive Enhancement

Both support progressive enhancement, but Turbo's approach is more opinionated with full-page fallbacks built-in.

### 4. WebSocket Support

HTMX uses SSE. Turbo Streams uses SignalR (WebSocket with fallbacks), providing:
- Automatic reconnection
- Better mobile support
- Built-in grouping/channels

## What You Gain

1. **Integrated real-time** - SignalR is more robust than SSE
2. **Simpler server responses** - Standard HTML, no special headers
3. **Built-in page navigation** - Full SPA-like experience
4. **Form handling** - Built-in error handling and redirects
5. **.NET Integration** - Tag helpers, strong typing

## What You Lose

1. **Flexibility** - HTMX works on any element
2. **HTTP verb control** - Need JavaScript for PUT/DELETE
3. **Fine-grained triggers** - Need JavaScript for complex triggers
4. **Extension ecosystem** - HTMX has more extensions

## Step-by-Step Migration

### 1. Add Turbo

```bash
dotnet add package Tombatron.Turbo
```

### 2. Replace Scripts

```html
<!-- Remove -->
<script src="htmx.min.js"></script>

<!-- Add -->
<script type="module" src="https://cdn.jsdelivr.net/npm/@hotwired/turbo@8/dist/turbo.es2017-esm.min.js"></script>
```

### 3. Convert Elements

For each `hx-*` element:

1. Wrap target area in `<turbo-frame id="...">`
2. Convert triggers to links or forms
3. Handle responses as partials

### 4. Convert SSE to Streams

1. Add SignalR script
2. Add `<turbo stream="...">` subscriptions
3. Convert server events to `ITurbo.Stream()` calls

### 5. Test Progressive Enhancement

Disable JavaScript and verify pages still work with full-page navigation.

## See Also

- [Turbo Frames Guide](../guides/turbo-frames.md)
- [Turbo Streams Guide](../guides/turbo-streams.md)
- [From Blazor Server](from-blazor-server.md)
