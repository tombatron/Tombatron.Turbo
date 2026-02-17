using Microsoft.EntityFrameworkCore;

namespace Tombatron.Turbo.Chat.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();
    public DbSet<UserRoomState> UserRoomStates => Set<UserRoomState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<RoomMember>(e =>
        {
            e.HasKey(rm => new { rm.RoomId, rm.UserId });
        });

        modelBuilder.Entity<UserRoomState>(e =>
        {
            e.HasKey(urs => new { urs.UserId, urs.RoomId });
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.HasIndex(m => m.RoomId);
        });

        // Seed default rooms
        modelBuilder.Entity<Room>().HasData(
            new Room { Id = 1, Name = "General", Description = "General discussion for everyone" },
            new Room { Id = 2, Name = "Random", Description = "Off-topic conversations" },
            new Room { Id = 3, Name = "Help", Description = "Get help with questions" }
        );
    }
}
