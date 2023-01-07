using Google.Cloud.Firestore;

namespace GalaxyBot.Data;

public class Firestore
{
    private readonly string _projectId;
    private readonly FirestoreDb _db;
    private readonly List<Gameplay> _gameplayCache = new();

    internal Firestore(Secrets secrets)
    {
        _projectId = secrets.FirestoreProject;
        _db = FirestoreDb.Create(_projectId);
        Console.WriteLine("Created Cloud Firestore client with project ID: {0}", _projectId);
    }

    public FirestoreDb Db
    {
        get
        {
            return _db;
        }
    }

    public List<Gameplay> GameplayCache
    {
        get
        {
            return _gameplayCache;
        }
    }

    public async Task RefreshGameplays()
    {

        var lastModified = _gameplayCache.Select(gameplay => gameplay.LastModified)
            .DefaultIfEmpty(DateTime.MinValue.ToUniversalTime())
            .Max();

        QuerySnapshot snapshot = await _db.Collection("gameplays")
            .WhereGreaterThan("LastModified", lastModified)
            .GetSnapshotAsync();

        foreach (var document in snapshot)
        {
            var gameplay = new Gameplay(document.ToDictionary());
            _gameplayCache.Add(gameplay);
        }
    }
}

