# Turbo Chat Sample

A real-time chat application demonstrating Turbo Streams with SignalR for instant messaging across multiple chat rooms.

## Running the Sample

```bash
cd samples/Tombatron.Turbo.Chat
dotnet run
```

Open https://localhost:5001 in your browser. Enter a username to join.

**Tip:** Open multiple browser windows with different usernames to see real-time messaging in action!

## Features Demonstrated

### 1. Room-Based Streams

Each chat room has its own SignalR stream. Users subscribe only to rooms they're viewing:

```html
<!-- Subscribe to room-specific stream -->
<turbo-stream-source-signalr stream="room:@Model.RoomId" hub-url="/turbo-hub">
</turbo-stream-source-signalr>
```

```csharp
// Broadcast message to room subscribers only
await _turbo.Stream($"room:{roomId}", builder =>
{
    builder.Append("messages", RenderMessage(message));
});
```

### 2. Real-Time Message Delivery

Messages are instantly delivered to all users in the same room:

```csharp
public async Task<IActionResult> OnPostSendMessage(string roomId, string content)
{
    // Add message to storage
    var message = _chatService.AddMessage(roomId, username, content);

    // Broadcast to all room subscribers
    await _turbo.Stream($"room:{roomId}", builder =>
    {
        builder.Append("messages", RenderMessage(message));
    });

    return new NoContentResult();
}
```

### 3. Typing Indicators

Shows when other users are typing in real-time:

```javascript
// Send typing start notification
messageInput.addEventListener('input', async function() {
    if (!isTyping) {
        isTyping = true;
        await fetch(`/Room/${roomId}?handler=StartTyping`, { method: 'POST' });
    }

    // Stop typing after 2 seconds of inactivity
    typingTimeout = setTimeout(async () => {
        isTyping = false;
        await fetch(`/Room/${roomId}?handler=StopTyping`, { method: 'POST' });
    }, 2000);
});
```

```csharp
// Server broadcasts typing indicator update
await _turbo.Stream($"room:{id}", builder =>
{
    builder.Update("typing-indicator", RenderTypingIndicator(id, username));
});
```

### 4. Multiple Actions in One Stream

A single stream message can update multiple DOM elements:

```csharp
await _turbo.Stream($"room:{roomId}", builder =>
{
    // Remove empty state message
    builder.Remove("empty-messages-placeholder");

    // Append the new message
    builder.Append("messages", RenderMessage(message));

    // Update typing indicator
    builder.Update("typing-indicator", RenderTypingIndicator(roomId, username));
});
```

### 5. Auto-Scroll on New Messages

JavaScript watches for DOM changes and scrolls to show new messages:

```javascript
const observer = new MutationObserver(() => {
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
});
observer.observe(document.getElementById('messages'), { childList: true });
```

## Project Structure

```
Tombatron.Turbo.Chat/
├── Program.cs                    # App setup with Turbo services
├── ChatService.cs                # In-memory chat storage
├── Pages/
│   ├── Index.cshtml              # Login/welcome page
│   ├── Index.cshtml.cs
│   ├── Room.cshtml               # Main chat interface
│   ├── Room.cshtml.cs            # Message handling
│   └── Shared/
│       ├── _Layout.cshtml        # Chat layout with styling
│       └── _Message.cshtml       # Message partial view
└── README.md
```

## Key Patterns

### Stream Naming Convention

Streams are named with a `room:` prefix for clarity:

```csharp
// Subscribe to room stream
<turbo-stream-source-signalr stream="room:general" ...>

// Broadcast to room
await _turbo.Stream("room:general", builder => { ... });
```

### HTML Escaping

User content is properly escaped to prevent XSS:

```csharp
var escapedContent = WebUtility.HtmlEncode(message.Content);
var escapedUsername = WebUtility.HtmlEncode(message.Username);
```

### NoContent Response

Forms return 204 No Content to prevent Turbo from navigating:

```csharp
// Turbo won't navigate; stream updates handle the UI
return new NoContentResult();
```

## Try It Out

1. Open in two browser windows
2. Enter different usernames in each
3. Join the same room
4. Send messages and watch them appear instantly in both windows
5. Start typing to see the typing indicator appear in the other window
6. Switch rooms to see room-specific messaging

## Learn More

- [Turbo Streams Guide](../../docs/guides/turbo-streams.md)
- [Authorization Guide](../../docs/guides/authorization.md)
