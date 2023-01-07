using Discord.WebSocket;

namespace GalaxyBot.Extensions
{
    internal static class DiscordSocketClientExt
    {
        public static SocketTextChannel? GetLogChannel(this DiscordSocketClient client, ulong logChannelId)
        {
            return (SocketTextChannel?)client.Guilds.SelectMany(guild => guild.Channels)
                .FirstOrDefault(channel => channel.Id == logChannelId);
        }
    }
}
