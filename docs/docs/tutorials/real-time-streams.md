---
title: "Real-Time Streams"
sidebar_label: "Real-Time Streams"
sidebar_position: 2
description: "Extend the todo app with real-time updates using Turbo Streams and SignalR."
---

In the [todo list tutorial](todo-list.md), Turbo Frames handle the request/response cycle — the user who submits the form sees the updated partial immediately, but nobody else does. Turbo Streams fix that by pushing updates over SignalR to every connected client.

This tutorial extends the todo example. Imagine two browsers open to the same todo list. When one user adds an item, the other browser should see it appear automatically.

## 1. Add a stream subscription to the page

Add a `<turbo>` tag to `Pages/Index.cshtml`. The `stream` attribute names the channel this page subscribes to — it must match the name used server-side in the next step. Place it outside the partial, since it's a separate concern from the frame-based form:

```html
@page
@model TurboTodo.Pages.IndexModel

<h1>Todo List</h1>

<turbo stream="todos"></turbo>

<partial name="_TodoList" model="Model" />
```

The `<turbo stream="todos">` tag helper renders a `<turbo-stream-source-signalr>` element that connects to the SignalR hub (configured by `MapTurboHub()`) and listens for messages on the `"todos"` stream.

## 2. Broadcast from the server

Inject `ITurbo` into the page model. After adding a todo, call `BroadcastRefresh()` to tell every connected client to re-fetch the page. Pass `HttpContext.GetSignalRConnectionId()` to exclude the submitter's SignalR connection from the broadcast — they already received the frame partial via the HTTP response, so the refresh would be redundant:

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

        // Tell every other connected client to refresh.
        // The submitter is excluded by connection ID.
        var connectionId = HttpContext.GetSignalRConnectionId();
        await _turbo.BroadcastRefresh(connectionId);

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

`BroadcastRefresh()` sends `<turbo-stream action="refresh">` to all clients over SignalR. The `excludedConnectionId` parameter prevents the submitter from receiving the broadcast — they already have the update from the frame response, so there's no double-update or duplicate items. The connection ID is sent automatically by the JS adapter as an `X-SignalR-Connection-Id` header on every Turbo fetch request (via the `turbo:before-fetch-request` event). Because each tab holds its own connection ID in memory, this works correctly with multiple browser tabs. If the header isn't present yet (e.g., initial page load before the SignalR connection is established), `GetSignalRConnectionId()` returns `null` and no exclusion is applied.

## 3. Update the remaining handlers

Apply the same pattern to `OnPostToggle` and `OnPostDelete`. Each handler mutates the data, broadcasts a refresh (excluding the submitter), then returns the frame partial:

```csharp
public async Task<IActionResult> OnPostToggle(int id)
{
    // ... toggle the todo's completed state ...

    var connectionId = HttpContext.GetSignalRConnectionId();
    await _turbo.BroadcastRefresh(connectionId);

    if (HttpContext.IsTurboFrameRequest())
    {
        return Partial("_TodoList", this);
    }

    return RedirectToPage();
}

public async Task<IActionResult> OnPostDelete(int id)
{
    // ... remove the todo ...

    var connectionId = HttpContext.GetSignalRConnectionId();
    await _turbo.BroadcastRefresh(connectionId);

    if (HttpContext.IsTurboFrameRequest())
    {
        return Partial("_TodoList", this);
    }

    return RedirectToPage();
}
```

The pattern is always the same: make the change, broadcast (excluding the submitter), return the partial. Every handler that mutates shared state should call `BroadcastRefresh()` so that all connected clients stay in sync.

## 4. Try it out

Run the application and open it in two browser windows side by side:

```bash
dotnet run
```

Open `https://localhost:5001` (or your configured URL) in two separate browser tabs or windows. Now add, toggle, or delete a todo in one window — the other window updates automatically. The submitter sees the instant frame response, while every other connected browser receives a refresh over SignalR and re-fetches the page to pick up the change.

This is the core of Turbo Streams: the user who made the change gets an immediate response via Turbo Frames, and everyone else gets a real-time push via SignalR. The `excludedConnectionId` parameter ensures the submitter doesn't receive a redundant broadcast.

## Stream actions

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

## Refresh (Turbo 8)

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

## Exclude originator by connection ID

When a user submits a form that triggers a Turbo Stream broadcast, they receive both the HTTP response *and* the SignalR broadcast — a duplicate. To prevent this, pass the originator's SignalR connection ID to exclude them from the broadcast:

```csharp
var connectionId = HttpContext.GetSignalRConnectionId();

await _turbo.Stream("room:1", builder =>
{
    builder.Append("messages", "<div>New message</div>");
}, connectionId);

// Also works with Broadcast and Refresh variants
await _turbo.Broadcast(builder => { ... }, connectionId);
await _turbo.StreamRefresh("room:1", connectionId);
await _turbo.BroadcastRefresh(connectionId);
```

The connection ID is automatically sent by the JS adapter as an `X-SignalR-Connection-Id` header on every Turbo fetch request. Each browser tab maintains its own connection ID in memory, so this works correctly across multiple tabs. The parameter is `string?` — passing `null` (e.g., on the initial page load before the SignalR connection is established) simply sends to all subscribers with no exclusion.

:::note
This is distinct from the `X-Turbo-Request-Id` mechanism used by `refresh` actions. Connection-ID exclusion prevents the SignalR message from being sent at all via `GroupExcept`/`AllExcept`, while request-ID suppression happens client-side.
:::

## Targeted vs. broadcast

```csharp
// Send to a specific stream (e.g., one user)
await _turbo.Stream($"user:{userId}", builder => { ... });

// Send to multiple streams
await _turbo.Stream(new[] { "stream-a", "stream-b" }, builder => { ... });

// Send to all connected clients
await _turbo.Broadcast(builder => { ... });
```
