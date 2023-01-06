using Discord.Interactions;
using Discord;
using GalaxyBot.Data;

namespace GalaxyBot.Modules.Games;

[Group("games", "Game related commands")]
public class SlashCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Firestore _firestore;

    public SlashCommand(Firestore firestore)
    {
        _firestore = firestore;
    }

    [RequireUserPermission(GuildPermission.ModerateMembers)]
    [SlashCommand("current", "Get the top 20 games currently being played by Galaxy members")]
    public async Task HandleCurrentCommand()
    {
        // Defers the interaction response until the command is finished executing
        await DeferAsync(true);

        // build a dictionary of games played with totals
        var server = Context.Guild;
        var users = server.Users.Where(user => user.IsBot == false).ToList();
        var games = new Dictionary<string, int>();

        foreach (var activity in users.SelectMany(user => user.Activities.Where(activity => activity.Type == ActivityType.Playing)))
        {
            if (games.ContainsKey(activity.Name))
            {
                games[activity.Name]++;
            }
            else
            {
                games.Add(activity.Name, 1);
            }
        }

        var gamesplayed = "";
        foreach (var game in games.OrderByDescending(game => game.Value).Take(20))
        {

            gamesplayed += $"{game.Key} - {game.Value}\n";
        }

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .AddField("Games currently being played by members:", gamesplayed)
            .Build();

        // Update the original message with the embed
        await FollowupAsync(embed: embed);
    }

    [RequireUserPermission(GuildPermission.ModerateMembers)]
    [SlashCommand("total", "Get the top 20 games played by Galaxy members")]
    public async Task HandleTotalCommand()
    {
        await DeferAsync(true);

        await _firestore.RefreshGameplays();

        var gameplayTotals = new Dictionary<string, double>();

        foreach (var gameplay in _firestore.GameplayCache)
        {
            var totalMinutes = (gameplay.EndTime - gameplay.StartTime).GetValueOrDefault().TotalHours;

            if (totalMinutes == 0)
            {
                continue;
            }

            if (gameplayTotals.ContainsKey(gameplay.Name))
            {
                gameplayTotals[gameplay.Name] += totalMinutes;
            }
            else
            {
                gameplayTotals.Add(gameplay.Name, totalMinutes);
            }
        }

        var gameplays = "";

        foreach (var gameplayTotal in gameplayTotals.OrderByDescending(totals => totals.Value).Take(20))
        {
            gameplays += $"{gameplayTotal.Key} - {gameplayTotal.Value:N2} hours\n\n";
        }

        if (string.IsNullOrEmpty(gameplays))
        {
            gameplays = "No full gameplays have been recorded";
        }

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .AddField("Games played in total minutes:", gameplays)
            .Build();

        await FollowupAsync(embed: embed);
    }
}
