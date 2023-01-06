using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GalaxyBot.Data;
using GalaxyBot.Handlers;
using GalaxyBot.Modules.Games;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GalaxyBot;

internal class Services
{
    /// <summary>
    /// Creates the bot services within a service provider.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static IServiceProvider CreateServiceProvider(string[] args)
    {
        var isProd = args.Contains("--production") || args.Contains("-P");

        // Discord configuration settings
        var discordConfig = Configuration.GetDiscordConfiguration();

        // Environmental configuration settings
        var environmentConfig = Configuration.GetEnvironmentConfiguration(isProd);

        // Bot Services
        var collection = new ServiceCollection()
            .AddSingleton(discordConfig)
            .AddSingleton(environmentConfig)
            .AddSingleton(_ => new DiscordSocketClient(discordConfig))
            .AddSingleton(_ => new Firestore(
                _.GetRequiredService<IConfigurationRoot>()))
            .AddSingleton(_ => new GameJobs(
                _.GetRequiredService<Firestore>(),
                _.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton(_ => new InteractionService(_.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>();

        return collection.BuildServiceProvider();
    }

    /// <summary>
    /// Initializes all bot services and starts the bot.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static async Task StartBot(string[] args)
    {
        // Get the IServiceProvider
        var serviceProvider = CreateServiceProvider(args);

        // Get the DiscordSocketClient from the IServiceProvider.
        var client = serviceProvider.GetRequiredService<DiscordSocketClient>();

        // Configure Slash Command Support
        var slashCommands = serviceProvider.GetRequiredService<InteractionService>();
        await serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();

        // Get the BotSettings from the IServiceProvider.
        var envConfig = serviceProvider.GetRequiredService<IConfigurationRoot>();

        // Intialize Game Jobs
        var gameJobs = serviceProvider.GetRequiredService<GameJobs>();

        // Set Log Handling
        client.Log += async msg => { await LogHandler.LogConsoleAsync(msg); };
        slashCommands.Log += async msg => { await LogHandler.LogConsoleAsync(msg); };

        // Refresh and load slash commands when the client is ready
        client.Ready += async () =>
        {
            SocketTextChannel logChannel;
            if (ulong.TryParse(envConfig["logChannelId"], out var logChannelId))
            {
                try
                {
                    logChannel = (SocketTextChannel)client.Guilds.SelectMany(guild => guild.Channels).First(channel => channel.Id == logChannelId);
                    client.Log += async msg => { await LogHandler.LogChannelAsync(logChannel, msg); };
                    slashCommands.Log += async msg => { await LogHandler.LogChannelAsync(logChannel, msg); };
                }
                catch (Exception)
                {
                    Console.Error.WriteLine("Log channel not found.");
                    Environment.Exit(1);
                }
            }
            else
            {
                Console.Error.WriteLine("logChannelId is missing or invalid.");
                Environment.Exit(1);
            };

            gameJobs.GetGameplayStatuses();

            foreach (var guild in client.Guilds)
            {
                List<SocketApplicationCommand> commands = new();
                commands.AddRange(await client.GetGlobalApplicationCommandsAsync());
                commands.AddRange(await guild.GetApplicationCommandsAsync());

                foreach (var cmd in commands) await cmd.DeleteAsync();
                await slashCommands.RegisterCommandsToGuildAsync(guild.Id);

            }
        };

        //Use GuildPresence to allow user status query with commands.  Using this to silence Warning.
        client.PresenceUpdated += (_, _, _) =>
            Task.CompletedTask;

        // Login and start the bot
        await client.LoginAsync(TokenType.Bot, envConfig["token"]);
        await client.StartAsync();

        // Wait infinitely so the bot actually stays running.
        await Task.Delay(-1);
    }
}
