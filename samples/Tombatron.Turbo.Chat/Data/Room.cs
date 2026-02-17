namespace Tombatron.Turbo.Chat.Data;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsDirectMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
