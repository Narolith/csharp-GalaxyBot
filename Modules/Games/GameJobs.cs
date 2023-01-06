using Discord;
using Discord.WebSocket;
using GalaxyBot.Data;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace GalaxyBot.Modules.Games;

public class GameJobs
{
    private readonly DiscordSocketClient _client;
    private readonly FirestoreDb _db;

    public GameJobs(Firestore firestore, DiscordSocketClient client)
    {
        _db = firestore.Db;
        _client = client;
        _client.PresenceUpdated += TrackGames;
    }

    /// <summary>
    /// Detects whether a user has started or stopped playing a game and adds it to the database.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="oldPresence"></param>
    /// <param name="newPresence"></param>
    /// <returns></returns>
    private async Task TrackGames(SocketUser user, SocketPresence oldPresence, SocketPresence newPresence)
    {
        IActivity? oldActivity = null;
        IActivity? newActivity = null;

        if (oldPresence.Activities != null)
        {
            oldActivity = oldPresence.Activities.FirstOrDefault(activity => activity.Type == ActivityType.Playing);
        }

        if (newPresence.Activities != null)
        {
            newActivity = newPresence.Activities.FirstOrDefault(activity => activity.Type == ActivityType.Playing);
        }

        if (oldActivity == null && newActivity != null)
        {
            // Started to play game
            var gameplay = new Gameplay(
                userId: user.Id,
                isActive: true,
                name: newActivity.Name,
                lastModified: DateTime.UtcNow,
                startTime: DateTime.UtcNow);

            await _db.Collection("gameplays")
                .Document(Guid.NewGuid().ToString())
                .SetAsync(gameplay.ToDictionary());
        }

        if (oldActivity != null && newActivity == null)
        {
            // Stopped playing game
            var collection = _db.Collection("gameplays");
            var snapshot = await collection
                .WhereEqualTo("UserId", user.Id)
                .WhereEqualTo("IsActive", true)
                .WhereEqualTo("Name", oldActivity.Name)
                .GetSnapshotAsync();

            if (snapshot.Documents.Count == 1)
            {
                var gameplay = new Gameplay(snapshot.Documents[0].ToDictionary())
                {
                    LastModified = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    IsActive = false
                };

                await collection
                    .Document(snapshot.Documents[0].Id)
                    .UpdateAsync(gameplay.ToDictionary());
            }


        }
    }

    /// <summary>
    /// Detects what users are playing games on startup and adds to the database if no unfinished game by the user is found.
    /// </summary>
    public async void GetGameplayStatuses()
    {
        var collection = _db.Collection("gameplays");

        var snapshot = await collection.WhereEqualTo("IsActive", true).GetSnapshotAsync();

        foreach (var guild in _client.Guilds)
        {
            foreach (var user in guild.Users.Where(user => user.IsBot == false))
            {
                bool unfinishedGameplay = false;
                var gameActivity = user.Activities?.FirstOrDefault(activity => activity.Type == ActivityType.Playing);

                foreach (var document in snapshot.Documents)
                {
                    var gameplay = new Gameplay(document.ToDictionary());

                    if (gameplay.UserId != user.Id)
                    {
                        continue;
                    }

                    if (gameplay.Name == gameActivity?.Name)
                    {
                        unfinishedGameplay = true;
                    }
                    else
                    {
                        gameplay.IsActive = false;
                        gameplay.LastModified = DateTime.UtcNow;
                        gameplay.EndTime = DateTime.UtcNow;
                        await collection.Document(document.Id).UpdateAsync(gameplay.ToDictionary());
                    }
                }

                if (unfinishedGameplay == false && gameActivity != null)
                {
                    var gameplay = new Gameplay(
                        userId: user.Id,
                        isActive: true,
                        name: gameActivity.Name,
                        lastModified: DateTime.UtcNow,
                        startTime: DateTime.UtcNow);

                    await collection
                        .Document(Guid.NewGuid().ToString())
                        .SetAsync(gameplay.ToDictionary());
                }
            }
        }


    }
}
