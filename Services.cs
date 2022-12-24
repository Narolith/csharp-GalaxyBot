using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GalaxyBot.Data;
using GalaxyBot.Handlers;
using GalaxyBot.Modules.Birthdays;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GalaxyBot
{
    internal class Services
    {
        internal static IServiceProvider CreateServiceProvider(string[] args)
        {
            var discordConfig = Configuration.GetDiscordConfiguration();

            var environmentConfig = Configuration.GetEnvironmentConfiguration(args);

            var collection = new ServiceCollection()
                .AddSingleton(discordConfig)
                .AddSingleton(environmentConfig)
                .AddDbContext<GalaxyBotContext>()
                .AddSingleton(_ => new DiscordSocketClient(discordConfig))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton(x => new BirthdayJobs(
                    x.GetRequiredService<GalaxyBotContext>(),
                    x.GetRequiredService<DiscordSocketClient>(),
                    x.GetRequiredService<IConfigurationRoot>())
                );

            return collection.BuildServiceProvider();
        }

        internal static async Task StartBotServices(string[] args)
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

            // Start Birthday Jobs
            var birthdayService = serviceProvider.GetRequiredService<BirthdayJobs>();
            new Thread(birthdayService.DailyBirthdayMessage).Start();

            // Set Log Handling
            client.Log += async msg => { await LogHandler.LogConsoleAsync(msg); };
            slashCommands.Log += async msg => { await LogHandler.LogConsoleAsync(msg); };

            // Refresh and load slash commands when the client is ready
            client.Ready += async () =>
            {
                var guild = client.GetGuild(ulong.Parse(envConfig["guildId"]));
                var globalCommands = await client.GetGlobalApplicationCommandsAsync();
                var guildCommands = await guild.GetApplicationCommandsAsync();

                foreach (var cmd in globalCommands) await cmd.DeleteAsync();

                foreach (var cmd in guildCommands) await cmd.DeleteAsync();

                await slashCommands.RegisterCommandsToGuildAsync(ulong.Parse(envConfig["guildId"]));
                if (guild.GetChannel(ulong.Parse(envConfig["logChannelId"])) is SocketTextChannel logChannel)
                {
                    client.Log += async msg => { await LogHandler.LogChannelAsync(logChannel, msg); };
                    slashCommands.Log += async msg => { await LogHandler.LogChannelAsync(logChannel, msg); };
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
}
