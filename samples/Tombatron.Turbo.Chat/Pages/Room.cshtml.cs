using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace Tombatron.Turbo.Chat.Pages;

[IgnoreAntiforgeryToken]
public class RoomModel : PageModel
{
    private readonly ChatService _chatService;
    private readonly ITurbo _turbo;

    public RoomModel(ChatService chatService, ITurbo turbo)
    {
        _chatService = chatService;
        _turbo = turbo;
    }

    public string RoomId { get; set; } = "general";
    public ChatRoom? Room { get; set; }
    public IEnumerable<ChatRoom> Rooms { get; set; } = Enumerable.Empty<ChatRoom>();
    public IReadOnlyList<ChatMessage> Messages { get; set; } = Array.Empty<ChatMessage>();
    public string Username { get; set; } = "Anonymous";

    public IActionResult OnGet(string id)
    {
        // Check if user is logged in
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToPage("/Index");
        }

        Username = username;
        RoomId = id;
        Room = _chatService.GetRoom(id);
        Rooms = _chatService.GetRooms();
        Messages = _chatService.GetMessages(id);

        if (Room == null)
        {
            return RedirectToPage("/Room", new { id = "general" });
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSendMessage(string roomId, string content)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(content))
        {
            return new NoContentResult();
        }

        // Add message to chat service
        var message = _chatService.AddMessage(roomId, username, content);

        // Broadcast to all subscribers of this room
        await _turbo.Stream($"room:{roomId}", builder =>
        {
            // Remove empty state message if present
            builder.Remove("empty-messages-placeholder");

            // Append the new message
            builder.Append("messages", RenderMessage(message));

            // Clear typing indicator for this user
            builder.Update("typing-indicator", RenderTypingIndicator(roomId, username));
        });

        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostStartTyping(string id)
    {
        var username = HttpContext.Session.GetString("Username");
        Console.WriteLine($"[StartTyping] id={id}, username={username}");

        if (string.IsNullOrEmpty(username))
        {
            Console.WriteLine("[StartTyping] No username in session, returning");
            return new NoContentResult();
        }

        _chatService.StartTyping(id, username);

        var typingUsers = _chatService.GetTypingUsers(id, null);
        Console.WriteLine($"[StartTyping] Typing users: {string.Join(", ", typingUsers)}");

        // Broadcast typing indicator to room
        var streamName = $"room:{id}";
        Console.WriteLine($"[StartTyping] Broadcasting to stream: {streamName}");

        await _turbo.Stream(streamName, builder =>
        {
            var html = RenderTypingIndicator(id, username);
            Console.WriteLine($"[StartTyping] Typing indicator HTML: {html}");
            builder.Update("typing-indicator", html);
        });

        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostStopTyping(string id)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return new NoContentResult();
        }

        _chatService.StopTyping(id, username);

        // Broadcast typing indicator update to room
        await _turbo.Stream($"room:{id}", builder =>
        {
            builder.Update("typing-indicator", RenderTypingIndicator(id, null));
        });

        return new NoContentResult();
    }

    private static string RenderMessage(ChatMessage message)
    {
        var initial = message.Username.Length > 0 ? message.Username[0].ToString().ToUpper() : "?";
        var time = message.Timestamp.ToLocalTime().ToString("h:mm tt");
        var escapedContent = WebUtility.HtmlEncode(message.Content);
        var escapedUsername = WebUtility.HtmlEncode(message.Username);

        return $@"
<div class=""message"" id=""message-{message.Id}"">
    <div class=""message-avatar"">{initial}</div>
    <div class=""message-content"">
        <div class=""message-header"">
            <span class=""message-username"">{escapedUsername}</span>
            <span class=""message-timestamp"">{time}</span>
        </div>
        <div class=""message-text"">{escapedContent}</div>
    </div>
</div>";
    }

    private string RenderTypingIndicator(string roomId, string? excludeUsername)
    {
        var typingUsers = _chatService.GetTypingUsers(roomId, excludeUsername);

        if (!typingUsers.Any())
        {
            return "";
        }

        var text = typingUsers.Count switch
        {
            1 => $"{typingUsers[0]} is typing...",
            2 => $"{typingUsers[0]} and {typingUsers[1]} are typing...",
            _ => "Several people are typing..."
        };

        return text;
    }
}
