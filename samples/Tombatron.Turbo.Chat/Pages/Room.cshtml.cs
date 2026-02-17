using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tombatron.Turbo.Chat.Data;
using Tombatron.Turbo.Generated;
using Tombatron.Turbo.Streams;

namespace Tombatron.Turbo.Chat.Pages;

[Authorize]
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

    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public List<Room> PublicRooms { get; set; } = new();
    public List<(Room Room, string OtherUsername)> DirectMessageRooms { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
    public List<Data.User> AllUsers { get; set; } = new();
    public Dictionary<int, int> UnreadCounts { get; set; } = new();
    public string Username => User.Identity!.Name!;
    public int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    public async Task<IActionResult> OnGet(int id)
    {
        RoomId = id;
        Room = await _chatService.GetRoom(id);

        if (Room is null)
        {
            return RedirectToPage("/Room", new { id = 1 });
        }

        await _chatService.JoinRoom(id, CurrentUserId);
        await _chatService.MarkRoomAsRead(CurrentUserId, id);

        Messages = await _chatService.GetMessages(id);
        PublicRooms = await _chatService.GetPublicRooms();
        DirectMessageRooms = await _chatService.GetDirectMessageRooms(CurrentUserId);
        AllUsers = await _chatService.GetAllUsers();
        UnreadCounts = await _chatService.GetAllUnreadCounts(CurrentUserId);

        return Page();
    }

    public async Task<IActionResult> OnPostSendMessage(int roomId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new NoContentResult();
        }

        var message = await _chatService.AddMessage(roomId, CurrentUserId, Username, content);

        // Broadcast message to room subscribers
        await _turbo.Stream($"room:{roomId}", async builder =>
        {
            builder.Remove("empty-messages-placeholder");
            await builder.AppendAsync("messages", Partials.Message, message);
            builder.Update("typing-indicator", RenderTypingIndicator(roomId));
        });

        // Broadcast unread badge updates to other room members
        var memberIds = await _chatService.GetRoomMemberUserIds(roomId);
        foreach (var memberId in memberIds)
        {
            if (memberId == CurrentUserId) continue;

            var unread = await _chatService.GetUnreadCount(memberId, roomId);
            await _turbo.Stream($"user:{memberId}", builder =>
            {
                builder.Update($"unread-badge-{roomId}",
                    unread > 0 ? $"<span class=\"unread-badge\">{unread}</span>" : "");
            });
        }

        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostCreateRoom(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new NoContentResult();
        }

        var room = await _chatService.CreatePublicRoom(name.Trim(), description?.Trim());
        await _chatService.JoinRoom(room.Id, CurrentUserId);

        // Broadcast new room to all connected clients
        await _turbo.Broadcast(async builder =>
        {
            await builder.AppendAsync("public-room-list", Partials.RoomEntry, (0, room, 0));
        });

        return RedirectToPage("/Room", new { id = room.Id });
    }

    public async Task<IActionResult> OnPostStartDm(int targetUserId)
    {
        var room = await _chatService.GetOrCreateDirectMessage(CurrentUserId, targetUserId);

        var targetUser = await _chatService.GetUserById(targetUserId);
        var currentUser = await _chatService.GetUserById(CurrentUserId);

        // Broadcast DM entry to target user
        if (targetUser is not null && currentUser is not null)
        {
            await _turbo.Stream($"user:{targetUserId}", async builder =>
            {
                await builder.AppendAsync("dm-room-list", Partials.DmEntry,
                    (0, currentUser.Username, room, 0));
            });
        }

        return RedirectToPage("/Room", new { id = room.Id });
    }

    public async Task<IActionResult> OnPostStartTyping(int id)
    {
        _chatService.StartTyping(id, Username);

        await _turbo.Stream($"room:{id}", builder =>
        {
            builder.Update("typing-indicator", RenderTypingIndicator(id));
        });

        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostStopTyping(int id)
    {
        _chatService.StopTyping(id, Username);

        await _turbo.Stream($"room:{id}", builder =>
        {
            builder.Update("typing-indicator", RenderTypingIndicator(id));
        });

        return new NoContentResult();
    }

    public async Task<IActionResult> OnGetUserProfile(int userId)
    {
        var user = await _chatService.GetUserById(userId);
        if (user is null) return NotFound();

        return Partial("_Profile", user);
    }

    private string RenderTypingIndicator(int roomId)
    {
        var typingUsers = _chatService.GetTypingUsers(roomId);
        var usersJson = System.Text.Json.JsonSerializer.Serialize(typingUsers);

        return $@"<span data-typing-users='{WebUtility.HtmlEncode(usersJson)}'></span>";
    }
}
