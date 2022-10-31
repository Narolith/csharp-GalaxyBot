using Discord;
using Discord.Interactions;

namespace GalaxyBot.Modules.Server
{
    public class SlashCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        [SlashCommand("server", "Get information about the current server")]
        public async Task HandleServerCommand()
        {
            await DeferAsync(ephemeral: true);

            var server = Context.Guild;

            var embed = new EmbedBuilder()
                .WithTitle(server.Name)
                .WithDescription(server.Description)
                .WithThumbnailUrl(server.IconUrl)
                .AddField("Owner", server.Owner)
                .AddField("Created", server.CreatedAt)
                .AddField("Online Members", server.Users.Count(user => user.Status != UserStatus.Offline))
                .AddField("Total Members", server.Users.Count)
                .Build();

            await FollowupAsync(embed: embed, ephemeral: true);
        }
    }
}
