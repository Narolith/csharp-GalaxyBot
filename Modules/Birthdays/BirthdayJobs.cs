using Discord;
using Discord.WebSocket;
using GalaxyBot.Data;
using Microsoft.Extensions.Configuration;

namespace GalaxyBot.Modules.Birthdays;

public class BirthdayJobs
{
    private readonly DiscordSocketClient _client;
    private readonly GalaxyBotContext _db;
    private readonly IConfigurationRoot _envConfig;

    public BirthdayJobs(GalaxyBotContext db, DiscordSocketClient client, IConfigurationRoot envConfig)
    {
        _db = db;
        _client = client;
        _envConfig = envConfig;

        _client.UserLeft += ClearBirthday;
    }

    private async Task ClearBirthday(SocketGuild guild, SocketUser user)
    {
        await Task.Run(() =>
        {
            var birthday = _db.Birthdays.FirstOrDefault(b => b.UserId == user.Id);
            if (birthday == null) return;
            _db.Birthdays.Remove(birthday);
            _db.SaveChanges();
        });
    }

    public void DailyBirthdayMessage()
    {
        const string dailyTime = "14:00:00";
        while (true)
        {
            //Time when method needs to be called
            var timeParts = dailyTime.Split(new[] { ':' });

            var dateNow = DateTime.UtcNow;
            var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day,
                int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));

            TimeSpan ts;
            if (date >= dateNow)
            {
                ts = date - dateNow;
            }
            else
            {
                date = date.AddDays(1);
                ts = date - dateNow;
            }

            //waits certain time and run the code
            var task = Task.Delay(ts).ContinueWith(async _ =>
            {
                var today = DateTime.Today;
                var users = _db.Birthdays.Where(b => b.Month == today.Month && b.Day == today.Day).Select(b => b.UserId)
                    .ToList();
                var guild = _client.Guilds.FirstOrDefault(g => g.Id == ulong.Parse(_envConfig["guildId"]));
                if (guild == null)
                {
                    await Console.Error.WriteLineAsync("DailyBirthdayMessage -> Guild not found");
                    return;
                }

                if (guild.Channels.FirstOrDefault(c => c.Id == guild.SystemChannel.Id) is not SocketTextChannel channel)
                {
                    await Console.Error.WriteLineAsync("DailyBirthdayMessage -> SystemChannel not found");
                    return;
                }

                var announcementRole =
                    guild.Roles.FirstOrDefault(role => role.Id == ulong.Parse(_envConfig["announcementRoleId"]));
                if (announcementRole == null)
                {
                    await Console.Error.WriteLineAsync("DailyBirthdayMessage -> AnnouncementRole not found");
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithTitle("Birthdays!")
                    .WithDescription(
                        $"{announcementRole.Mention}, Please wish a happy birthday to:\n\n{string.Join("\n", users.Select(u => guild.GetUser(u).Mention))}")
                    .WithThumbnailUrl("https://i.imgur.com/2LQPTEO.png")
                    .Build();

                await channel.SendMessageAsync(embed: embed);
            });
            task.Wait();
        } 
    }
}