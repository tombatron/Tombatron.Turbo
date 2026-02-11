# @tombatron/turbo-signalr

SignalR adapter for [Hotwire Turbo](https://turbo.hotwired.dev/) Streams. Enables real-time DOM updates via ASP.NET Core SignalR.

## Installation

### npm

```bash
npm install @tombatron/turbo-signalr @microsoft/signalr
```

### CDN

```html
<!-- SignalR (required) -->
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8/dist/browser/signalr.min.js"></script>

<!-- Turbo SignalR adapter -->
<script src="https://cdn.jsdelivr.net/npm/@tombatron/turbo-signalr/dist/turbo-signalr.js"></script>
```

### NuGet (ASP.NET Core)

If using the [Tombatron.Turbo](https://www.nuget.org/packages/Tombatron.Turbo/) NuGet package, a bundled version is included:

```html
<script src="_content/Tombatron.Turbo/turbo-signalr.bundled.min.js"></script>
```

## Usage

### HTML Custom Element

```html
<!-- Subscribe to a stream -->
<turbo-stream-source-signalr
    stream="notifications"
    hub-url="/turbo-hub">
</turbo-stream-source-signalr>

<!-- Target element for updates -->
<div id="notifications"></div>
```

### With Turbo.js (Recommended)

When Turbo.js is loaded, stream messages are automatically rendered:

```html
<script type="module" src="https://cdn.jsdelivr.net/npm/@hotwired/turbo@8/dist/turbo.es2017-esm.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8/dist/browser/signalr.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/@tombatron/turbo-signalr/dist/turbo-signalr.js"></script>

<turbo-stream-source-signalr stream="my-stream" hub-url="/turbo-hub"></turbo-stream-source-signalr>
```

### Without Turbo.js

The adapter includes a fallback renderer that handles basic Turbo Stream actions when Turbo.js is not present.

## Attributes

| Attribute | Required | Default | Description |
|-----------|----------|---------|-------------|
| `stream` | Yes | - | Stream name to subscribe to |
| `hub-url` | No | `/turbo-hub` | SignalR hub URL |

## Events

The adapter dispatches events on the `document`:

| Event | Description |
|-------|-------------|
| `turbo:signalr:connected` | Connection established |
| `turbo:signalr:disconnected` | Connection closed |
| `turbo:signalr:reconnecting` | Attempting to reconnect |
| `turbo:signalr:reconnected` | Successfully reconnected |
| `turbo:signalr:error` | Connection error (includes `error` in detail) |

Element-level events (bubble up):

| Event | Description |
|-------|-------------|
| `turbo:stream:unauthorized` | Subscription denied |
| `turbo:stream:error` | Subscription error |

## Programmatic API

```javascript
import { connectionManager, getConnectionState, disconnect } from '@tombatron/turbo-signalr';

// Check connection state
const state = getConnectionState();
console.log(state.isConnected, state.streams);

// Manually disconnect
await disconnect();
```

## Server-Side (ASP.NET Core)

This adapter works with [Tombatron.Turbo](https://github.com/tombatron/Tombatron.Turbo):

```csharp
// Broadcast updates to subscribers
await turbo.Stream("notifications", builder =>
{
    builder.Append("notifications", "<div>New message!</div>");
});
```

## Features

- **Singleton Connection**: Multiple elements share one SignalR connection
- **Reference Counting**: Automatically manages subscriptions
- **Auto-Reconnect**: Exponential backoff reconnection with resubscription
- **Turbo.js Integration**: Seamless rendering when Turbo is present
- **Fallback Renderer**: Works without Turbo.js for basic operations

## Browser Support

Works in all modern browsers that support:
- Custom Elements (Web Components)
- ES6 Modules
- WebSocket (or SignalR's fallback transports)

## License

MIT
