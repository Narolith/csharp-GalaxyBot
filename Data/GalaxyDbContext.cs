using Microsoft.EntityFrameworkCore;

namespace GalaxyBot.Data
{

    public class GalaxyBotContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Birthday> Birthdays => Set<Birthday>();

        private string DbPath { get; }

        public GalaxyBotContext()
        {
            DbPath = Path.Join(AppContext.BaseDirectory, "GalaxyBot.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
    
    public abstract class User
    {
        public ulong UserId { get; set; }
        public string Username { get; set; } = null!;
    }

    public class Birthday
    {
        public int BirthdayId { get; set; }
        public ulong UserId { get; init; }
        public int Day { get; set; }
        public int Month { get; set; }
    }
}
