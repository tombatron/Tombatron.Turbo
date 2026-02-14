namespace Tombatron.Turbo.Chat;

/// <summary>
/// Simple in-memory chat service for the demo.
/// In a real application, this would be backed by a database.
/// </summary>
public class ChatService
{
    private readonly Dictionary<string, ChatRoom> _rooms = new() { ["general"] = new ChatRoom("general", "General", "General discussion for everyone"), ["random"] = new ChatRoom("random", "Random", "Off-topic conversations"), ["help"] = new ChatRoom("help", "Help", "Get help with questions") };

    private readonly Dictionary<string, HashSet<string>> _typingUsers = new();
    private readonly object _lock = new();

    public IEnumerable<ChatRoom> GetRooms(string? username = null)
    {
        return username is null ? _rooms.Values.Where(x => x.Members is null).ToList() :
            _rooms.Values.Where(x => x.Members is null || x.Members.Exists(m => m.Username == username));
    }

    public ChatRoom? GetRoom(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    public ChatMessage AddMessage(string roomId, string username, string content)
    {
        lock (_lock)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                throw new ArgumentException($"Room '{roomId}' not found", nameof(roomId));
            }

            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                RoomId = roomId,
                Username = username,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            room.Messages.Add(message);

            // Keep only last 100 messages per room
            if (room.Messages.Count > 100)
            {
                room.Messages.RemoveAt(0);
            }

            // Clear typing indicator when message is sent
            StopTyping(roomId, username);

            return message;
        }
    }

    public IReadOnlyList<ChatMessage> GetMessages(string roomId, int count = 50)
    {
        lock (_lock)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                return Array.Empty<ChatMessage>();
            }

            return room.Messages.TakeLast(count).ToList();
        }
    }

    public void StartTyping(string roomId, string username)
    {
        lock (_lock)
        {
            if (!_typingUsers.ContainsKey(roomId))
            {
                _typingUsers[roomId] = new HashSet<string>();
            }

            _typingUsers[roomId].Add(username);
        }
    }

    public void StopTyping(string roomId, string username)
    {
        lock (_lock)
        {
            if (_typingUsers.TryGetValue(roomId, out var users))
            {
                users.Remove(username);
            }
        }
    }

    public IReadOnlyList<string> GetTypingUsers(string roomId)
    {
        lock (_lock)
        {
            if (!_typingUsers.TryGetValue(roomId, out var users))
            {
                return Array.Empty<string>();
            }

            return users.OrderBy(u => u).ToList();
        }
    }

    public ChatRoom CreateRoom(string name, string description, List<UserProfile>? members = null)
    {
        lock (_lock)
        {
            var id = Guid.NewGuid().ToString("N")[..8];

            if (_rooms.ContainsKey(id))
            {
                throw new InvalidOperationException($"Room '{id}' already exists");
            }

            _rooms[id] = new ChatRoom(id, name, description, members);

            return _rooms[id];
        }
    }

    public UserProfile GetUserProfile(string username)
    {
        return new UserProfile { Username = username };
    }
}

public class ChatRoom(string id, string name, string description, List<UserProfile>? members = null)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public List<ChatMessage> Messages { get; } = new();
    public List<UserProfile>? Members { get; } = members;
}

public class ChatMessage
{
    public string Id { get; set; } = "";
    public string RoomId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

public class UserProfile
{
    public required string Username { get; set; }
}
