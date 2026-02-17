using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Tombatron.Turbo.Chat.Data;

namespace Tombatron.Turbo.Chat;

public class ChatService
{
    private readonly ChatDbContext _db;

    // Typing is inherently transient — static in-memory is fine
    private static readonly Dictionary<int, HashSet<string>> _typingUsers = new();
    private static readonly object _typingLock = new();

    public ChatService(ChatDbContext db)
    {
        _db = db;
    }

    // ── Auth ──

    public async Task<User> RegisterUser(string username, string password)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<User?> ValidateCredentials(string username, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
        {
            return null;
        }

        return VerifyPassword(password, user.PasswordHash) ? user : null;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserById(int userId)
    {
        return await _db.Users.FindAsync(userId);
    }

    // ── Rooms ──

    public async Task<List<Room>> GetPublicRooms()
    {
        return await _db.Rooms.Where(r => !r.IsDirectMessage).OrderBy(r => r.Id).ToListAsync();
    }

    public async Task<List<(Room Room, string OtherUsername)>> GetDirectMessageRooms(int userId)
    {
        var dmRoomIds = await _db.RoomMembers
            .Where(rm => rm.UserId == userId)
            .Join(_db.Rooms.Where(r => r.IsDirectMessage), rm => rm.RoomId, r => r.Id, (rm, r) => r.Id)
            .ToListAsync();

        var results = new List<(Room, string)>();

        foreach (var roomId in dmRoomIds)
        {
            var room = await _db.Rooms.FindAsync(roomId);

            if (room is null)
            {
                continue;
            }

            var otherUsername = await _db.RoomMembers
                .Where(rm => rm.RoomId == roomId && rm.UserId != userId)
                .Join(_db.Users, rm => rm.UserId, u => u.Id, (rm, u) => u.Username)
                .FirstOrDefaultAsync() ?? "Unknown";

            results.Add((room, otherUsername));
        }

        return results;
    }

    public async Task<Room?> GetRoom(int roomId)
    {
        return await _db.Rooms.FindAsync(roomId);
    }

    public async Task<Room> CreatePublicRoom(string name, string? description)
    {
        var room = new Room
        {
            Name = name,
            Description = description,
            IsDirectMessage = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        return room;
    }

    public async Task<Room> GetOrCreateDirectMessage(int userId1, int userId2)
    {
        // Find existing DM room between these two users
        var existingRoomId = await _db.RoomMembers
            .Where(rm => rm.UserId == userId1)
            .Select(rm => rm.RoomId)
            .Intersect(
                _db.RoomMembers
                    .Where(rm => rm.UserId == userId2)
                    .Select(rm => rm.RoomId)
            )
            .Join(_db.Rooms.Where(r => r.IsDirectMessage), id => id, r => r.Id, (id, r) => r.Id)
            .FirstOrDefaultAsync();

        if (existingRoomId > 0)
        {
            return (await _db.Rooms.FindAsync(existingRoomId))!;
        }

        var user1 = await _db.Users.FindAsync(userId1);
        var user2 = await _db.Users.FindAsync(userId2);

        var room = new Room
        {
            Name = $"DM: {user1?.Username}, {user2?.Username}",
            IsDirectMessage = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        _db.RoomMembers.AddRange(
            new RoomMember { RoomId = room.Id, UserId = userId1 },
            new RoomMember { RoomId = room.Id, UserId = userId2 }
        );
        await _db.SaveChangesAsync();

        return room;
    }

    // ── Messages ──

    public async Task<Message> AddMessage(int roomId, int userId, string username, string content)
    {
        var message = new Message
        {
            RoomId = roomId,
            UserId = userId,
            Username = username,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        StopTyping(roomId, username);

        return message;
    }

    public async Task<List<Message>> GetMessages(int roomId, int count = 50)
    {
        return await _db.Messages
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.Id)
            .Take(count)
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    // ── Membership ──

    public async Task JoinRoom(int roomId, int userId)
    {
        var exists = await _db.RoomMembers
            .AnyAsync(rm => rm.RoomId == roomId && rm.UserId == userId);

        if (!exists)
        {
            _db.RoomMembers.Add(new RoomMember { RoomId = roomId, UserId = userId });
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<int>> GetRoomMemberUserIds(int roomId)
    {
        return await _db.RoomMembers
            .Where(rm => rm.RoomId == roomId)
            .Select(rm => rm.UserId)
            .ToListAsync();
    }

    // ── Unread ──

    public async Task MarkRoomAsRead(int userId, int roomId)
    {
        var lastMessageId = await _db.Messages
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.Id)
            .Select(m => m.Id)
            .FirstOrDefaultAsync();

        var state = await _db.UserRoomStates.FindAsync(userId, roomId);

        if (state is null)
        {
            _db.UserRoomStates.Add(new UserRoomState
            {
                UserId = userId,
                RoomId = roomId,
                LastReadMessageId = lastMessageId
            });
        }
        else
        {
            state.LastReadMessageId = lastMessageId;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCount(int userId, int roomId)
    {
        var state = await _db.UserRoomStates.FindAsync(userId, roomId);
        var lastRead = state?.LastReadMessageId ?? 0;

        return await _db.Messages.CountAsync(m => m.RoomId == roomId && m.Id > lastRead);
    }

    public async Task<Dictionary<int, int>> GetAllUnreadCounts(int userId)
    {
        var states = await _db.UserRoomStates
            .Where(s => s.UserId == userId)
            .ToDictionaryAsync(s => s.RoomId, s => s.LastReadMessageId);

        var roomIds = await _db.RoomMembers
            .Where(rm => rm.UserId == userId)
            .Select(rm => rm.RoomId)
            .Union(_db.Rooms.Where(r => !r.IsDirectMessage).Select(r => r.Id))
            .Distinct()
            .ToListAsync();

        var result = new Dictionary<int, int>();

        foreach (var roomId in roomIds)
        {
            var lastRead = states.GetValueOrDefault(roomId, 0);
            var count = await _db.Messages.CountAsync(m => m.RoomId == roomId && m.Id > lastRead);

            if (count > 0)
            {
                result[roomId] = count;
            }
        }

        return result;
    }

    // ── Users ──

    public async Task<List<User>> GetAllUsers()
    {
        return await _db.Users.OrderBy(u => u.Username).ToListAsync();
    }

    // ── Typing (static in-memory) ──

    public void StartTyping(int roomId, string username)
    {
        lock (_typingLock)
        {
            if (!_typingUsers.ContainsKey(roomId))
            {
                _typingUsers[roomId] = new HashSet<string>();
            }

            _typingUsers[roomId].Add(username);
        }
    }

    public void StopTyping(int roomId, string username)
    {
        lock (_typingLock)
        {
            if (_typingUsers.TryGetValue(roomId, out var users))
            {
                users.Remove(username);
            }
        }
    }

    public IReadOnlyList<string> GetTypingUsers(int roomId)
    {
        lock (_typingLock)
        {
            if (!_typingUsers.TryGetValue(roomId, out var users))
            {
                return Array.Empty<string>();
            }

            return users.OrderBy(u => u).ToList();
        }
    }

    // ── Password Hashing ──

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100_000, 32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');

        if (parts.Length != 2)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var computedHash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100_000, 32);

        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}
