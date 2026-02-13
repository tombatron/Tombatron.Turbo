using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Tombatron.Turbo.Generated;
using Tombatron.Turbo.Streams;

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

        // Broadcast to all subscribers of this room using the new ergonomic async syntax
        await _turbo.Stream($"room:{roomId}", async builder =>
        {
            // Remove empty state message if present
            builder.Remove("empty-messages-placeholder");

            // Append the new message using the PartialTemplate directly
            await builder.AppendAsync("messages", Partials.Message, message);

            // Update typing indicator (user already removed from typing list by AddMessage)
            builder.Update("typing-indicator", RenderTypingIndicator(roomId));
        });

        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostStartTyping(string id)
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return new NoContentResult();
        }

        _chatService.StartTyping(id, username);

        // Broadcast typing indicator to room (don't exclude anyone - all users should see who's typing)
        await _turbo.Stream($"room:{id}", builder =>
        {
            builder.Update("typing-indicator", RenderTypingIndicator(id));
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
            builder.Update("typing-indicator", RenderTypingIndicator(id));
        });

        return new NoContentResult();
    }

    private string RenderTypingIndicator(string roomId)
    {
        var typingUsers = _chatService.GetTypingUsers(roomId);
        var usersJson = System.Text.Json.JsonSerializer.Serialize(typingUsers);

        // Send data attributes - client JS will filter and render
        return $@"<span data-typing-users='{WebUtility.HtmlEncode(usersJson)}'></span>";
    }
}
