using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GalaxyBot.Data;
using GalaxyBot.Handlers;
using GalaxyBot.Modules.Birthdays;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GalaxyBot
{
    public class Program
    {
        private readonly IServiceProvider _serviceProvider;

        private Program(string[] args)
        {
            _serviceProvider = CreateProvider(args);
        }

        private static void Main(string[] args)
            => new Program(args).RunAsync().GetAwaiter().GetResult();

        private static IServiceProvider CreateProvider(string[] args)
        {
            IConfigurationRoot envConfig;

            var config = new DiscordSocketConfig();


            // Import configuration from devConfig.yaml if dev arg is passed
            if (args.Contains("--production") || args.Contains("-P"))
            {
                envConfig = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddYamlFile("Configs/prodConfig.yml", optional: false, reloadOnChange: true)
                    .Build();
            }
            // Import production configuration from prodConfig.yml by default
            else
            {
                envConfig = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddYamlFile("Configs/devConfig.yml", optional: false, reloadOnChange: true)
                    .Build();
            }

            var collection = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton(envConfig)
                .AddDbContext<GalaxyBotContext>()
                .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 1000,
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildPresences |
                                     GatewayIntents.GuildMessages | GatewayIntents.GuildMembers
                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton(x => new BirthdayJobs(
                    x.GetRequiredService<GalaxyBotContext>(),
                    x.GetRequiredService<DiscordSocketClient>(),
                    x.GetRequiredService<IConfigurationRoot>())
                );

            return collection.BuildServiceProvider();
        }

        /// <summary>
        /// This method runs the bot.
        /// </summary>
        private async Task RunAsync()
        {
            // Get the DiscordSocketClient from the IServiceProvider.
            var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

            // Configure Slash Command Support
            var slashCommands = _serviceProvider.GetRequiredService<InteractionService>();
            await _serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();

            // Get the BotSettings from the IServiceProvider.
            var envConfig = _serviceProvider.GetRequiredService<IConfigurationRoot>();

            // Start Birthday Jobs
            var birthdayService = _serviceProvider.GetRequiredService<BirthdayJobs>();
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

                foreach (var cmd in globalCommands)
                {
                    await cmd.DeleteAsync();
                }

                foreach (var cmd in guildCommands)
                {
                    await cmd.DeleteAsync();
                }

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

        // Have a CreateHostBuilder that simply returns a default builder.
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Console.WriteLine("Doing Entity Framework migrations stuff, not starting full application");
            return Host.CreateDefaultBuilder();
        }
    }
}