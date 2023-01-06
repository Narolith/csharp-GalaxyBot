using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

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

    internal static IConfigurationRoot GetEnvironmentConfiguration(bool isProd)
    {
        var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory);
        try
        {
            // Import configuration from prodConfig.json if production arg is passed
            if (isProd)
                configuration.AddJsonFile("Configs/prodConfig.json", false, true)
                             .Build();
            // Import production configuration from devConfig.json by default
            else
                configuration.AddJsonFile("Configs/devConfig.json", false, true)
                             .Build();
        }
        catch (FileNotFoundException exception)
        {
            Console.WriteLine(exception.Message);
            Environment.Exit(1);
        }

        return configuration.Build();
    }
}

