using Discord;
using Discord.WebSocket;
using GalaxyBot.Data;
using System.Text.Json;

namespace GalaxyBot;

internal class Configuration
{
    internal static DiscordSocketConfig GetDiscordConfiguration()
    {
        return new DiscordSocketConfig()
        {
            LogLevel = LogSeverity.Info,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 1000,
            GatewayIntents = GatewayIntents.Guilds
                             | GatewayIntents.GuildPresences
                             | GatewayIntents.GuildMessages
                             | GatewayIntents.GuildMembers
        };
    }

    internal static Secrets GetEnvironmentConfiguration(bool isProd)
    {

        string stream;
        if (isProd)
        {
            stream = File.ReadAllText("Configs/prodConfig.json");
        }
        else
        {
            stream = File.ReadAllText("Configs/devConfig.json");
        }

        return JsonSerializer.Deserialize<Secrets>(stream) ?? new Secrets();

    }
}

