# Tombatron.Turbo Sample Application

This sample demonstrates the core features of Tombatron.Turbo: Turbo Frames for partial page updates and Turbo Streams for real-time updates via SignalR.

## Running the Sample

```bash
cd samples/Tombatron.Turbo.Sample
dotnet run
```

Then open https://localhost:5001 in your browser.

## Features Demonstrated

### 1. Turbo Frames (Cart Page)

The Cart page demonstrates Turbo Frames for partial page updates without full page reloads.

**Key Concepts:**

- **Dynamic Frame IDs with Prefix**: Each cart item uses a dynamic ID (`item_@item.Id`) with the `asp-frame-prefix` attribute for compile-time validation
- **Frame-based Updates**: Adding/removing items updates only the affected frames
- **Turbo Stream Responses**: Form submissions return Turbo Stream HTML to update multiple targets

**Code Highlights:**

```html
<!-- Dynamic frame with prefix for compile-time validation -->
<turbo-frame id="item_@item.Id" asp-frame-prefix="item_">
    <!-- Item content -->
</turbo-frame>
```

```csharp
// Return Turbo Stream response for form submission
if (Request.Headers.Accept.ToString().Contains("text/vnd.turbo-stream.html"))
{
    var response = new StringBuilder();

    // Remove the item
    response.AppendLine($@"<turbo-stream action=""remove"" target=""item_{itemId}""></turbo-stream>");

    // Update the summary
    response.AppendLine($@"<turbo-stream action=""replace"" target=""cart-summary"">
        <template>{RenderCartSummary()}</template>
    </turbo-stream>");

    return Content(response.ToString(), "text/vnd.turbo-stream.html");
}
```

### 2. Turbo Streams (Streams Page)

The Streams page demonstrates real-time updates via SignalR.

**Key Concepts:**

- **SignalR Custom Element**: The `<turbo-stream-source-signalr>` element manages the SignalR connection
- **Stream Subscriptions**: Clients subscribe to named streams and receive updates in real-time
- **Multiple Update Types**: Append, Update, and Broadcast operations
- **Connection Status**: Visual indicator shows connection state

**Code Highlights:**

```html
<!-- Subscribe to a named stream -->
<turbo-stream-source-signalr stream="demo-notifications" hub-url="/turbo-hub">
</turbo-stream-source-signalr>
```

```csharp
// Send update to stream subscribers
await _turbo.Stream("demo-notifications", builder =>
{
    builder.Append("notification-list",
        $"<div class=\"notification\">{message}</div>");
});

// Broadcast to all connected clients
await _turbo.Broadcast(builder =>
{
    builder.Append("broadcast-list",
        $"<div class=\"broadcast\">{message}</div>");
});
```

### 3. Connection Status Indicator

The layout includes a connection status indicator that updates based on SignalR events:

```javascript
document.addEventListener('turbo:signalr:connected', () => {
    updateStatusIndicator('connected');
});

document.addEventListener('turbo:signalr:disconnected', () => {
    updateStatusIndicator('disconnected');
});
```

## Project Structure

```
Tombatron.Turbo.Sample/
├── Program.cs                    # Application setup with Turbo services
├── Pages/
│   ├── Index.cshtml              # Home page
│   ├── Index.cshtml.cs
│   ├── Cart/
│   │   ├── Index.cshtml          # Shopping cart with Turbo Frames
│   │   └── Index.cshtml.cs       # Cart logic with Turbo Stream responses
│   ├── Streams/
│   │   ├── Index.cshtml          # Real-time streams demo
│   │   └── Index.cshtml.cs       # Stream broadcasting logic
│   └── Shared/
│       ├── _Layout.cshtml        # Main layout with Turbo.js and SignalR
│       └── _WelcomeMessage.cshtml
└── wwwroot/                      # Static files
```

## Setup Code

### Program.cs

```csharp
// Add Turbo services
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";
});

// ... other middleware ...

// Use Turbo middleware (adds Turbo-Frame header detection)
app.UseTurbo();

// Map the SignalR hub
app.MapTurboHub();
```

### Layout (Required Scripts)

```html
<!-- Renders Turbo.js + SignalR adapter script tags -->
<turbo-scripts />
```

## Try It Out

1. **Cart Demo**: Add and remove items to see Turbo Frames in action
2. **Streams Demo**: Open in multiple tabs and send notifications to see real-time sync
3. **Counter**: Click increment in one tab and watch it update in all tabs
4. **Broadcast**: Send a message to all connected clients

## Learn More

- [Turbo Frames Guide](../../docs/guides/turbo-frames.md)
- [Turbo Streams Guide](../../docs/guides/turbo-streams.md)
- [API Documentation](../../docs/api/)
