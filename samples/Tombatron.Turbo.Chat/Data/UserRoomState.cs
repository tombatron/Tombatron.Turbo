namespace Tombatron.Turbo.Chat.Data;

public class UserRoomState
{
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public int LastReadMessageId { get; set; }
}
