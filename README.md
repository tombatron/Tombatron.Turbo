# Tombatron.Turbo

[![Build and Test](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml/badge.svg)](https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Tombatron.Turbo.svg)](https://www.nuget.org/packages/Tombatron.Turbo/)
[![npm](https://img.shields.io/npm/v/@tombatron/turbo-signalr.svg)](https://www.npmjs.com/package/@tombatron/turbo-signalr)

Hotwire Turbo for ASP.NET Core with SignalR-powered real-time streams.

## Features

- **Turbo Frames** — Partial page updates with automatic `Turbo-Frame` header detection
- **Turbo Streams** — Real-time updates via SignalR with targeted and broadcast support
- **Stimulus** — Convention-based controller discovery with import maps and hot reload
- **Source Generator** — Compile-time strongly-typed partial references
- **Form Validation** — HTTP 422 support for inline validation errors within Turbo Frames
- **Minimal API Support** — Return partials from Minimal API endpoints with `TurboResults`
- **Import Maps** — Pin JavaScript modules with `<turbo-scripts mode="Importmap" />`
- **Zero Configuration** — Works out of the box with Turbo.js

## Tutorial: Build a Todo List

This walkthrough creates a todo list app from scratch using Turbo Frames for partial page updates and Stimulus for client-side behavior. Each step builds on the previous one.

### Step 1 — Create the project and install packages

```bash
dotnet new webapp -n TurboTodo
cd TurboTodo
dotnet add package Tombatron.Turbo
dotnet add package Tombatron.Turbo.Stimulus
```

### Step 2 — Configure services

Replace the contents of `Program.cs`:

```csharp
using Tombatron.Turbo;
using Tombatron.Turbo.Stimulus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTurbo();
builder.Services.AddStimulus();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseTurbo();

app.MapRazorPages();
app.MapTurboHub();

app.Run();
```

`AddTurbo()` registers the Turbo services and tag helpers. `AddStimulus()` sets up automatic controller discovery from `wwwroot/controllers/`. `UseTurbo()` adds middleware that sets the `Vary` header on Turbo Frame responses. `MapTurboHub()` exposes the SignalR hub for Turbo Streams.

### Step 3 — Register tag helpers

Add to `Pages/_ViewImports.cshtml`:

```razor
@addTagHelper *, Tombatron.Turbo
```

### Step 4 — Set up the layout

Replace `Pages/Shared/_Layout.cshtml`:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Todo List</title>
    <turbo-scripts mode="Importmap" />
    <style>
        body { font-family: system-ui, sans-serif; max-width: 600px; margin: 2rem auto; padding: 0 1rem; }
        .todo-item { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 0; }
        .completed { text-decoration: line-through; opacity: 0.6; }
        .error { color: red; font-size: 0.875rem; }
        input[type="text"] { flex: 1; padding: 0.5rem; font-size: 1rem; }
        button { padding: 0.5rem 1rem; cursor: pointer; }
    </style>
</head>
<body>
    @RenderBody()
</body>
</html>
```

The `<turbo-scripts mode="Importmap" />` tag helper renders Turbo.js, the SignalR bridge, Stimulus, and any discovered controllers via an import map.

### Step 5 — Create the page model

Replace `Pages/Index.cshtml.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tombatron.Turbo;

namespace TurboTodo.Pages;

public record TodoItem(int Id, string Title, bool IsComplete);

public class IndexModel : PageModel
{
    private static readonly List<TodoItem> _todos = new()
    {
        new(1, "Learn Turbo Frames", false),
        new(2, "Add Stimulus controllers", false)
    };

    private static int _nextId = 3;

    public List<TodoItem> Todos => _todos;
    public string? Error { get; set; }

    public void OnGet() { }

    public IActionResult OnPostAdd(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            Error = "Title is required.";
            Response.StatusCode = 422;
            return Partial("_TodoList", this);
        }

        _todos.Add(new TodoItem(_nextId++, title.Trim(), false));

        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_TodoList", this);
        }

        return RedirectToPage();
    }

    public IActionResult OnPostToggle(int id)
    {
        var index = _todos.FindIndex(t => t.Id == id);

        if (index >= 0)
        {
            var todo = _todos[index];
            _todos[index] = todo with { IsComplete = !todo.IsComplete };
        }

        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_TodoList", this);
        }

        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        _todos.RemoveAll(t => t.Id == id);

        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_TodoList", this);
        }

        return RedirectToPage();
    }
}
```

The pattern is straightforward: check `IsTurboFrameRequest()` and return just the partial, or redirect for regular requests. When validation fails, set HTTP 422 so Turbo replaces the frame content in-place.

### Step 6 — Create the page view

Replace `Pages/Index.cshtml`:

```html
@page
@model TurboTodo.Pages.IndexModel

<h1>Todo List</h1>

<partial name="_TodoList" model="Model" />
```

The page renders the `_TodoList` partial, which wraps everything in a `<turbo-frame id="todo-list">`. When a form inside the frame submits, Turbo sends the request with a `Turbo-Frame: todo-list` header and replaces the frame with the partial response.

### Step 7 — Create the todo list partial

Create `Pages/Shared/_TodoList.cshtml`:

```html
@model TurboTodo.Pages.IndexModel

<turbo-frame id="todo-list">
    <form method="post" asp-page-handler="Add"
          data-controller="todo-form"
          data-action="turbo:submit-end->todo-form#reset">
        <div style="display: flex; gap: 0.5rem;">
            <input type="text" name="title" placeholder="What needs to be done?"
                   data-todo-form-target="input" />
            <button type="submit">Add</button>
        </div>
        @if (Model.Error is not null)
        {
            <p class="error">@Model.Error</p>
        }
    </form>

    @foreach (var todo in Model.Todos)
    {
        <div class="todo-item">
            <form method="post" asp-page-handler="Toggle">
                <input type="hidden" name="id" value="@todo.Id" />
                <button type="submit">@(todo.IsComplete ? "✓" : "○")</button>
            </form>
            <span class="@(todo.IsComplete ? "completed" : "")">@todo.Title</span>
            <form method="post" asp-page-handler="Delete">
                <input type="hidden" name="id" value="@todo.Id" />
                <button type="submit">×</button>
            </form>
        </div>
    }
</turbo-frame>
```

The partial wraps everything in a `<turbo-frame>` with the same `id` as the page. When Turbo receives the response, it matches the frame by ID and swaps the content.

If validation fails (HTTP 422), Turbo replaces the frame content with the error markup instead of navigating away.

### Step 8 — Add a Stimulus controller

Create `wwwroot/controllers/todo_form_controller.js`:

```javascript
import { Controller } from "@hotwired/stimulus";

export default class extends Controller {
    static targets = ["input"];

    reset(event) {
        if (event.detail.success) {
            this.inputTarget.value = "";
        }
    }
}
```

This controller clears the input field after a successful submission. The naming convention maps the filename to an identifier: `todo_form_controller.js` becomes `todo-form` (underscores become hyphens, the `_controller.js` suffix is stripped).

The `data-controller="todo-form"` attribute on the form connects it, and `data-action="turbo:submit-end->todo-form#reset"` calls the `reset` method when Turbo finishes the submission.

No manual registration is needed. `AddStimulus()` automatically discovers controllers in `wwwroot/controllers/` and generates the import map entries.

### Step 9 — Run it

```bash
dotnet run
```

Open `https://localhost:5001` (or the port shown in the console). You should be able to add, toggle, and delete todos without full page reloads. The form clears automatically on successful submission thanks to the Stimulus controller. If you submit an empty title, the validation error appears inline.

## Real-Time Updates with Turbo Streams

In the tutorial above, Turbo Frames handle the request/response cycle — the user who submits the form sees the updated partial immediately, but nobody else does. Turbo Streams fix that by pushing updates over SignalR to every connected client.

This section extends the todo example. Imagine two browsers open to the same todo list. When one user adds an item, the other browser should see it appear automatically.

### 1. Add a stream subscription to the page

Add a `<turbo>` tag to `Pages/Index.cshtml`. The `stream` attribute names the channel this page subscribes to — it must match the name used server-side in the next step. Place it outside the partial, since it's a separate concern from the frame-based form:

```html
@page
@model TurboTodo.Pages.IndexModel

<h1>Todo List</h1>

<turbo stream="todos"></turbo>

<partial name="_TodoList" model="Model" />
```

The `<turbo stream="todos">` tag helper renders a `<turbo-stream-source-signalr>` element that connects to the SignalR hub (configured by `MapTurboHub()`) and listens for messages on the `"todos"` stream.

### 2. Broadcast from the server

Inject `ITurbo` into the page model. After adding a todo, call `BroadcastRefresh()` to tell every connected client to re-fetch the page. The submitter is automatically suppressed — Turbo captures the `X-Turbo-Request-Id` header from the request so the originating browser skips the redundant refresh (it already received the frame partial via the HTTP response):

```csharp
using Tombatron.Turbo;

public class IndexModel : PageModel
{
    private readonly ITurbo _turbo;

    public IndexModel(ITurbo turbo)
    {
        _turbo = turbo;
    }

    public async Task<IActionResult> OnPostAdd(string? title)
    {
        // ... validation and add the todo (same as before) ...

        // Tell every connected client to refresh. The submitter is
        // automatically suppressed via X-Turbo-Request-Id.
        await _turbo.BroadcastRefresh();

        // Return the frame partial as usual — the refresh handles other
        // clients, and the frame response handles the submitter.
        if (HttpContext.IsTurboFrameRequest())
        {
            return Partial("_TodoList", this);
        }

        return RedirectToPage();
    }
}
```

`BroadcastRefresh()` sends `<turbo-stream action="refresh">` to all clients over SignalR. Other browsers re-fetch the page and see the new todo. The submitter already has the update from the frame response, so the refresh is suppressed for them — no double-update, no duplicate items, regardless of which stream action you use.

### Stream actions

All eight Turbo Stream actions are supported:

```csharp
await _turbo.Stream("my-stream", builder =>
{
    builder
        .Append("list", "<div>New item</div>")    // Add to end
        .Prepend("list", "<div>First</div>")       // Add to beginning
        .Replace("item-1", "<div>Updated</div>")   // Replace entire element
        .Update("count", "42")                      // Replace inner content
        .Remove("old-item")                         // Remove element
        .Before("btn", "<div>Before</div>")         // Insert before element
        .After("btn", "<div>After</div>")           // Insert after element
        .Refresh("request-id");                     // Tell clients to re-fetch the page
});
```

### Refresh (Turbo 8)

The `refresh` stream action tells clients to re-fetch their current page instead of receiving rendered HTML. The originator (the client whose request triggered the change) is automatically suppressed via the `X-Turbo-Request-Id` header, preventing a double-update.

```csharp
// Convenience: auto-extracts request-id from the current request
await _turbo.BroadcastRefresh();
await _turbo.StreamRefresh("room:123");
await _turbo.StreamRefresh(new[] { "room:123", "room:456" });

// Manual: within a builder callback
await _turbo.Broadcast(builder => builder.Refresh(HttpContext.GetTurboRequestId()));

// No suppression: all clients refresh
await _turbo.Broadcast(builder => builder.Refresh());
```

### Targeted vs. broadcast

```csharp
// Send to a specific stream (e.g., one user)
await _turbo.Stream($"user:{userId}", builder => { ... });

// Send to multiple streams
await _turbo.Stream(new[] { "stream-a", "stream-b" }, builder => { ... });

// Send to all connected clients
await _turbo.Broadcast(builder => { ... });
```

## Reference

### Turbo Frames

Check for a Turbo Frame request and return a partial:

```csharp
if (HttpContext.IsTurboFrameRequest())
{
    return Partial("_MyPartial", Model);
}
```

Lazy-load a frame by setting its `src`:

```html
<turbo-frame id="comments" src="/posts/1?handler=Comments" loading="lazy">
    Loading...
</turbo-frame>
```

### Stimulus

`AddStimulus()` discovers controllers from `wwwroot/controllers/` (default) and registers them in the import map. No manual registration required.

**Naming conventions:**

| File | Identifier |
|---|---|
| `hello_controller.js` | `hello` |
| `todo_form_controller.js` | `todo-form` |
| `admin/users_controller.js` | `admin--users` |
| `admin/user_settings_controller.js` | `admin--user-settings` |

**Options:**

```csharp
builder.Services.AddStimulus(options =>
{
    options.ControllersPath = "js/controllers";        // Default: "controllers"
    options.StimulusCdnUrl = "https://unpkg.com/...";  // Default: stimulus 3.2.2 from unpkg
    options.EnableHotReload = true;                     // Default: auto-detect from environment
});
```

Hot reload is enabled automatically in Development — save a controller file and the browser picks up the changes.

### Tag Helpers

Register in `_ViewImports.cshtml`:

```razor
@addTagHelper *, Tombatron.Turbo
```

**`<turbo-scripts>`** — Renders Turbo.js, SignalR bridge, and Stimulus:

```html
<turbo-scripts />                       <!-- Traditional <script> tags -->
<turbo-scripts mode="Importmap" />      <!-- Import map with modulepreload -->
```

**`<turbo-frame>`** — Turbo Frame element:

```html
<turbo-frame id="my-frame" src="/load" loading="lazy"></turbo-frame>
```

**`<turbo>`** — Subscribe to Turbo Streams:

```html
<turbo stream="notifications"></turbo>
<turbo stream="user:@User.Identity.Name"></turbo>
```

### Import Maps

Pin additional modules in `Program.cs`:

```csharp
builder.Services.AddTurbo(options =>
{
    options.ImportMap.Pin("my-lib", "/js/my-lib.js", preload: true);
    options.ImportMap.Unpin("turbo-signalr"); // Remove a default pin
});
```

Default pins (set automatically):
- `@hotwired/turbo` → Turbo.js 8.x from unpkg (preloaded)
- `turbo-signalr` → Bundled SignalR bridge from NuGet (preloaded)

When using `AddStimulus()`, the Stimulus library and a generated controller index are also pinned automatically.

### Minimal API

Return partials from Minimal API endpoints:

```csharp
app.MapGet("/items", (HttpContext ctx) =>
{
    if (ctx.IsTurboFrameRequest())
    {
        return TurboResults.Partial("_Items", model);
    }
    return Results.Redirect("/");
});
```

### Form Validation

Return HTTP 422 to replace frame content in-place with validation errors:

**Razor Pages:**

```csharp
if (!ModelState.IsValid)
{
    Response.StatusCode = 422;
    return Partial("_Form", this);
}
```

**Minimal API:**

```csharp
return TurboResults.ValidationFailure("_Form", new { Errors = "Name is required." });
```

### Configuration

```csharp
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";                             // Default: "/turbo-hub"
    options.AddVaryHeader = true;                               // Default: true
    options.UseSignedStreamNames = true;                        // Default: true
    options.SignedStreamNameExpiration = TimeSpan.FromHours(24); // Default: 24 hours
    options.EnableAutoReconnect = true;                         // Default: true
    options.MaxReconnectAttempts = 5;                           // Default: 5
    options.DefaultUserStreamPattern = "user:{0}";              // Default: "user:{0}"
    options.DefaultSessionStreamPattern = "session:{0}";        // Default: "session:{0}"
});
```

### Helper Extensions

```csharp
// Is this a Turbo Frame request?
HttpContext.IsTurboFrameRequest()

// Is it for a specific frame?
HttpContext.IsTurboFrameRequest("cart-items")

// Does the frame ID start with a prefix?
HttpContext.IsTurboFrameRequestWithPrefix("item_")

// Get the raw frame ID
string? frameId = HttpContext.GetTurboFrameId();

// Is this a Turbo Stream request?
HttpContext.IsTurboStreamRequest()
```

### Source Generator

The source generator is bundled with `Tombatron.Turbo` — no extra package needed. It scans `_*.cshtml` partial views at compile time and generates a `Partials` class (in `Tombatron.Turbo.Generated`) with strongly-typed references:

```csharp
// Instead of magic strings (requires IPartialRenderer):
await builder.AppendAsync("messages", renderer, "_Message", message);

// Use generated references (no renderer needed):
await builder.AppendAsync("messages", Partials.Message, message);
```

## Sample Applications

**[Tombatron.Turbo.Sample](samples/Tombatron.Turbo.Sample)** — Turbo Frames, Turbo Streams, shopping cart, and form validation demo.

**[Tombatron.Turbo.Chat](samples/Tombatron.Turbo.Chat)** — Real-time chat with cookie auth, SQLite, rooms, DMs, unread indicators, and Stimulus controllers.

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
