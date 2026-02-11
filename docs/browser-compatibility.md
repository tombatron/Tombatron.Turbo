# Browser Compatibility

This document covers browser support and compatibility considerations for Tombatron.Turbo.

## Supported Browsers

The client-side JavaScript requires:
- Custom Elements (Web Components)
- SignalR JavaScript client
- ES2015+ JavaScript

### Desktop Browsers

| Browser | Minimum Version | Status |
|---------|-----------------|--------|
| Chrome | 67+ | Fully supported |
| Firefox | 63+ | Fully supported |
| Safari | 10.1+ | Fully supported |
| Edge | 79+ (Chromium) | Fully supported |
| Edge Legacy | 16-18 | Requires polyfills |
| Internet Explorer | - | Not supported |

### Mobile Browsers

| Browser | Minimum Version | Status |
|---------|-----------------|--------|
| Chrome (Android) | 67+ | Fully supported |
| Safari (iOS) | 10.3+ | Fully supported |
| Firefox (Android) | 63+ | Fully supported |
| Samsung Internet | 9.0+ | Fully supported |

## Feature Requirements

### Custom Elements

The `<turbo-stream-source-signalr>` component uses the Custom Elements API:

```javascript
class TurboStreamSourceSignalR extends HTMLElement {
    connectedCallback() { /* ... */ }
    disconnectedCallback() { /* ... */ }
}
customElements.define('turbo-stream-source-signalr', TurboStreamSourceSignalR);
```

**Polyfill for older browsers:**
```html
<script src="https://unpkg.com/@webcomponents/custom-elements"></script>
```

### SignalR Transport

SignalR supports multiple transports with automatic fallback:

1. **WebSockets** (preferred) - Best performance, real-time
2. **Server-Sent Events** - Fallback for when WebSockets unavailable
3. **Long Polling** - Final fallback for older environments

The library automatically selects the best available transport.

### WebSockets

WebSockets are supported in all modern browsers. Potential issues:
- Corporate proxies may block WebSocket connections
- Some load balancers require specific configuration

**Testing WebSocket availability:**
```javascript
if ('WebSocket' in window) {
    console.log('WebSockets supported');
}
```

## Turbo.js Compatibility

Tombatron.Turbo is designed to work with Hotwire Turbo:

```html
<!-- Include Turbo.js -->
<script type="module">
    import * as Turbo from 'https://esm.sh/@hotwired/turbo@8';
</script>

<!-- Include the SignalR streaming component -->
<script src="_content/Tombatron.Turbo/dist/turbo-signalr.bundled.min.js"></script>
```

### Turbo.js Version Compatibility

| Turbo.js Version | Status |
|------------------|--------|
| 8.x | Fully supported |
| 7.x | Fully supported |

## Known Issues

### iOS Safari WebSocket Disconnect

iOS Safari may disconnect WebSocket connections when the app is backgrounded. The library handles this with automatic reconnection:

```javascript
// Automatic resubscription on reconnect
connection.onreconnected = () => {
    // All active streams are resubscribed
};
```

### Corporate Proxy Blocking

Some corporate proxies block WebSocket connections. SignalR will fall back to Server-Sent Events or Long Polling automatically.

For environments where this is common, you can force a specific transport:

```javascript
// Force Long Polling (not recommended for performance)
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/turbo-stream", { transport: signalR.HttpTransportType.LongPolling })
    .build();
```

### Ad Blockers

Some ad blockers may interfere with SignalR connections. Test with common ad blockers:
- uBlock Origin
- AdBlock Plus
- Brave Shield

## Testing Recommendations

### Cross-Browser Testing

Test your application in:
1. Chrome (latest)
2. Firefox (latest)
3. Safari (latest)
4. Edge (latest)
5. Mobile Chrome (Android)
6. Mobile Safari (iOS)

### Tools for Testing

- **BrowserStack** - Cross-browser testing platform
- **Sauce Labs** - Automated browser testing
- **Playwright** - End-to-end testing across browsers

### Playwright Example

```javascript
const { chromium, firefox, webkit } = require('playwright');

for (const browserType of [chromium, firefox, webkit]) {
    const browser = await browserType.launch();
    const context = await browser.newContext();
    const page = await context.newPage();

    await page.goto('http://localhost:5000');

    // Wait for SignalR connection
    await page.waitForFunction(() =>
        window.turboSignalRConnection?.state === 'Connected'
    );

    // Test stream updates
    // ...

    await browser.close();
}
```

## Fallback Strategies

### No JavaScript

For users with JavaScript disabled, Turbo Frames still work with standard navigation:

```html
<turbo-frame id="content">
    <!-- Content loads via standard HTTP request -->
    <a href="/page">Load Content</a>
</turbo-frame>
```

Turbo Streams require JavaScript and will not function without it. Consider:
- Progressive enhancement for critical features
- Server-side alternatives for essential updates

### No WebSocket Support

If WebSockets are unavailable, SignalR falls back automatically. For very old browsers, consider:

```html
<!-- Polling fallback for critical updates -->
<turbo-frame id="notifications" src="/api/notifications"
             data-turbo-refresh-method="morph"
             data-turbo-refresh-scroll="preserve">
    <!-- Poll for updates instead of streaming -->
</turbo-frame>
```

## Performance Considerations by Browser

### Chrome
- Best WebSocket performance
- Efficient JavaScript execution
- Best DevTools for debugging

### Firefox
- Excellent WebSocket support
- Good performance overall
- Useful network debugging tools

### Safari
- WebSocket connections may be limited on mobile
- Background tabs may disconnect
- Test thoroughly on iOS

### Edge
- Performance similar to Chrome (same engine)
- Good enterprise compatibility
