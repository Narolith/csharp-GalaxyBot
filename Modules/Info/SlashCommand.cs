using Discord;
using Discord.Interactions;

namespace GalaxyBot.Modules.Info;

/// <summary>
///     Info command module.
/// </summary>
public class SlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    /// <summary>
    ///     Info slash command that displays information about the selected user.
    /// </summary>
    /// <param name="user">The user to display information about. If null, will use the user who called the command</param>
    [RequireUserPermission(GuildPermission.ModerateMembers)]
    [SlashCommand("info", "Get information about yourself or another user")]
    public async Task HandleInfoCommand(
        [Summary(description: "User to look up information on (Defaults to you if blank)")] IUser? user = null)
    {
        // Defers the interaction response until the command is finished executing
        await DeferAsync(true);

        // If no user is specified, use the user who invoked the command
        user ??= Context.User;

        // Build an embed with user information
        var embed = new EmbedBuilder()
            .WithAuthor(user)
            .WithThumbnailUrl(user.GetAvatarUrl())
            .AddField("Username", user.Username, true)
            .AddField("Discriminator", user.Discriminator, true)
            .AddField("ID", user.Id, true)
            .AddField("Created at", user.CreatedAt.ToString("dd/MM/yyyy"), true)
            .AddField("Joined at", (user as IGuildUser)?.JoinedAt?.ToString("dd/MM/yyyy") ?? "N/A", true)
            .AddField("Status", (user as IGuildUser)?.Status.ToString() ?? "N/A", true)
            .AddField("Bot", user.IsBot, true)
            .AddField("Mention", user.Mention, true)
            .WithColor(Color.Blue)
            .Build();

        // Update the original message with the embed
        await FollowupAsync(embed: embed);
    }
}