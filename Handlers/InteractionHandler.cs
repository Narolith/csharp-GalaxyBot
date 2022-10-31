using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;

namespace GalaxyBot.Handlers;

/// <summary>
///     Handler responsible for processing interactions.
/// </summary>
public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;

    public InteractionHandler(
        DiscordSocketClient client,
        InteractionService interactionService,
        IServiceProvider services)
    {
        _client = client;
        _commands = interactionService;
        _services = services;
    }

    /// <summary>
    ///     Registers all slash commands.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += HandleInteractionAsync;
    }

    /// <summary>
    ///     Handles interactions.
    /// </summary>
    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _commands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
}