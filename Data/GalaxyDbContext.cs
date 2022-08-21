using Microsoft.EntityFrameworkCore;
using GalaxyBot.Data.Models;
using GalaxyBot.Modules.Lfg.Models;

namespace GalaxyBot.Data
{

    public class GalaxyBotContext : DbContext
    {
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<GroupUser> GroupUsers => Set<GroupUser>();
        public DbSet<Birthday> Birthdays => Set<Birthday>();

        public string DbPath { get; }

        public GalaxyBotContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "GalaxyBot.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }

    public class Group
    {
        public int GroupId { get; set; }
        public ulong GroupLeaderId { get; set; }
        public int NumOfHealers { get; set; }
        public int NumOfTanks { get; set; }
        public int NumOfDps { get; set; }
    }
    
    public class GroupUser
    {
        public int GroupUserId { get; set; }
        public int GroupId { get; set; }
        public ulong UserId { get; set; }
        public PlayerClass Class { get; set; }
    }

    public class Role
    {
        public int RoleId { get; set; }
        public int GroupId { get; set; }
        public ulong UserId { get; set; }
        public JobType JobType { get; set; }
    }

    public class User
    {
        public ulong UserId { get; set; }
        public string Username { get; set; } = null!;
    }

    public class Birthday
    {
        public int BirthdayId { get; set; }
        public ulong UserId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
    }
}
