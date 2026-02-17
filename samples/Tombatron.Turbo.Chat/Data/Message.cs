namespace Tombatron.Turbo.Chat.Data;

public class Message
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
