namespace Tombatron.Turbo.Chat.Data;

public class RoomMember
{
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
