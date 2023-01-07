using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using GalaxyBot.Extensions;
using GalaxyBot.Handlers;
using GalaxyBot.Modules.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GalaxyBot.Data;

var IsProduction = args.Contains("--production") || args.Contains("-P");

// Build host with bot services
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddBotServices(IsProduction))
    .Build();

// Get service provider
using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider serviceProvider = serviceScope.ServiceProvider;

// Initialize interaction handler
await serviceProvider.GetRequiredService<InteractionHandler>().InitializeAsync();

// Grab secrets
var secrets = serviceProvider.GetRequiredService<Secrets>();

// Grab client and slash command services
var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
var slashCommands = serviceProvider.GetRequiredService<InteractionService>();

// Set console logs
client.Log += async msg => { await msg.LogAsync(); };
slashCommands.Log += async msg => { await msg.LogAsync(); };

client.Ready += async () =>
{
    if (!ulong.TryParse(secrets.LogChannelId, out var logChannelId))
    {
        Console.Error.WriteLine("logChannelId is missing or invalid.");
        Environment.Exit(1);
    }
    var logChannel = client.GetLogChannel(logChannelId);

    if (logChannel == null)
    {
        Console.Error.WriteLine("Log channel not found.");
        Environment.Exit(1);
    }

    // Set channel logs
    client.Log += async msg => { await msg.LogAsync(logChannel); };
    slashCommands.Log += async msg => { await msg.LogAsync(logChannel); };

    // Grab initial gameplay statuses
    serviceProvider.GetRequiredService<GameJobs>()
    .GetGameplayStatuses();

    // Refresh slash commands
    foreach (var guild in client.Guilds)
    {
        List<SocketApplicationCommand> commands = new();
        commands.AddRange(await client.GetGlobalApplicationCommandsAsync());
        commands.AddRange(await guild.GetApplicationCommandsAsync());

        foreach (var cmd in commands) await cmd.DeleteAsync();
        await slashCommands.RegisterCommandsToGuildAsync(guild.Id);

    }
};

await client.LoginAsync(TokenType.Bot, secrets.Token);
await client.StartAsync();

await Task.Delay(-1);