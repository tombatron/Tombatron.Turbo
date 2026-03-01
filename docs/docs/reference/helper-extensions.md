---
title: Helper Extensions
sidebar_label: Helper Extensions
sidebar_position: 2
description: HttpContext extension methods for detecting Turbo request types.
---

Tombatron.Turbo provides extension methods on `HttpContext` for detecting Turbo request types and retrieving connection metadata.

## Turbo Frame Extensions

```csharp
// Is this a Turbo Frame request?
HttpContext.IsTurboFrameRequest()

// Is it for a specific frame?
HttpContext.IsTurboFrameRequest("cart-items")

// Does the frame ID start with a prefix?
HttpContext.IsTurboFrameRequestWithPrefix("item_")

// Get the raw frame ID
string? frameId = HttpContext.GetTurboFrameId();
```

## Turbo Stream Extensions

```csharp
// Is this a Turbo Stream request?
HttpContext.IsTurboStreamRequest()
```

## SignalR Extensions

```csharp
// Get the SignalR connection ID (for originator exclusion)
string? connectionId = HttpContext.GetSignalRConnectionId();
```

The connection ID is sent automatically by the JS adapter as an `X-SignalR-Connection-Id` header on every Turbo fetch request. Pass it to `ITurbo.Stream()` or `ITurbo.Broadcast()` to exclude the originator from broadcasts.

## Usage Pattern

The typical pattern in a page handler:

```csharp
public IActionResult OnPostUpdate()
{
    // Mutate data...

    if (HttpContext.IsTurboFrameRequest())
    {
        return Partial("_MyPartial", Model);
    }

    return RedirectToPage();
}
```

For more details, see the [Turbo Frames Guide](../guides/turbo-frames.md).
