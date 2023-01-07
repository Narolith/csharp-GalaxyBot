using System.Text.Json.Serialization;

namespace GalaxyBot.Data;

public class Secrets
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
    [JsonPropertyName("logChannelId")]
    public string LogChannelId { get; set; } = string.Empty;
    [JsonPropertyName("firestoreProject")]
    public string FirestoreProject { get; set; } = string.Empty;
}
