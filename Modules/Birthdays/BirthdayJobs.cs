using Discord;
using Discord.WebSocket;
using GalaxyBot.Data;
using Microsoft.Extensions.Configuration;

namespace GalaxyBot.Modules.Birthdays
{
    public class BirthdayJobs
    {
        private readonly GalaxyBotContext _db;
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _envConfig;

        public BirthdayJobs(GalaxyBotContext db, DiscordSocketClient client, IConfigurationRoot envConfig)
        {
            _db = db;
            _client = client;
            _envConfig = envConfig;

            _client.UserLeft += ClearBirthday;
        }
         
        public async Task ClearBirthday(SocketGuild guild, SocketUser user)
        {
            await Task.Run(() =>
            {
                var birthday = _db.Birthdays.FirstOrDefault(b => b.UserId == user.Id);
                if (birthday != null)
                {
                    _db.Birthdays.Remove(birthday);
                    _db.SaveChanges();
                }
            });
        }

        public void DailyBirthdayMessage()
        {
            while (true)
            {
                //Time when method needs to be called
                var DailyTime = "14:00:00";
                var timeParts = DailyTime.Split(new char[1] { ':' });

                var dateNow = DateTime.UtcNow;
                var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day,
                            int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));

                TimeSpan ts;
                if (date >= dateNow)
                    ts = date - dateNow;
                else
                {
                    date = date.AddDays(1);
                    ts = date - dateNow;
                }

                //waits certain time and run the code
                var task = Task.Delay(ts).ContinueWith(async (x) =>
                {
                    var today = DateTime.Today;
                    var users = _db.Birthdays.Where(b => b.Month == today.Month && b.Day == today.Day).Select(b => b.UserId).ToList();
                    var guild = _client.Guilds.FirstOrDefault(g => g.Id == ulong.Parse(_envConfig["guildId"]));
                    if (guild == null)
                    {
                        Console.Error.WriteLine("DailyBirthdayMessage -> Guild not found");
                        return;
                    }
                    if (guild.Channels.FirstOrDefault(c => c.Id == guild.SystemChannel.Id) is not SocketTextChannel channel)
                    {
                        Console.Error.WriteLine("DailyBirthdayMessage -> SystemChannel not found");
                        return;
                    }
                    var galaxians = guild.Roles.FirstOrDefault(role => role.Id == ulong.Parse(_envConfig["announcementRoleId"]));
                    if (galaxians == null)
                    {
                        Console.Error.WriteLine("DailyBirthdayMessage -> AnnouncementRole not found");
                        return;
                    }
                    var embed = new EmbedBuilder()
                    .WithTitle("Birthdays!")
                    .WithDescription($"{galaxians.Mention}, Please wish a happy birthday to:\n\n{string.Join("\n", users.Select(u => guild.GetUser(u).Mention))}")
                    .WithThumbnailUrl("https://i.imgur.com/2LQPTEO.png")
                    .Build();
                    
                    await channel.SendMessageAsync(embed: embed);
                });
                task.Wait();
            }
        }
    }
}
