namespace GalaxyBot.Data;

public class Gameplay
{
    public ulong UserId { get; set; }
    public bool IsActive { get; set; }
    public string Name { get; set; } = null!;
    public DateTime LastModified { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public Gameplay(Dictionary<string, object> keyValuePairs)
    {
        if (ulong.TryParse(keyValuePairs["UserId"].ToString(), out ulong userId))
        {
            UserId = userId;
        }
        else
        {
            throw new ArgumentException("A valid userId was not provided");
        }
        IsActive = (bool)keyValuePairs["IsActive"];
        Name = (string)keyValuePairs["Name"];
        LastModified = ((Google.Cloud.Firestore.Timestamp)keyValuePairs["LastModified"]).ToDateTime();
        StartTime = ((Google.Cloud.Firestore.Timestamp)keyValuePairs["StartTime"]).ToDateTime();
        if (keyValuePairs["EndTime"] != null)
        {
            EndTime = ((Google.Cloud.Firestore.Timestamp)keyValuePairs["EndTime"]).ToDateTime();
        }
    }

    public Gameplay(ulong userId, bool isActive, string name, DateTime lastModified, DateTime startTime, DateTime? endTime = null)
    {
        UserId = userId;
        IsActive = isActive;
        Name = name;
        StartTime = startTime;
        EndTime = endTime;
        LastModified = lastModified;
    }

    public IDictionary<string, object?> ToDictionary()
    {
        var dictionary = new Dictionary<string, object?>
        {
            { "UserId", UserId },
            { "Name", Name },
            { "IsActive", IsActive },
            { "LastModified", LastModified },
            { "StartTime", StartTime }
        };

        if (EndTime != null)
        {
            dictionary.Add("EndTime", EndTime);
        }
        else
        {
            dictionary.Add("EndTime", null);
        }

        return dictionary;
    }
}

