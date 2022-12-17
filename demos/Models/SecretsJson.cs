using System.Text.Json.Serialization;

namespace demos.Models;

public class SecretsJson
{
    [JsonPropertyName("appId")]
    public string AppId { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("tenant")]
    public string? Tenant { get; set; }

    [JsonPropertyName("objectId")]
    public string? ObjectId { get; set; }

    [JsonPropertyName("keyvaultName")]
    public string? KeyVaultName { get; set; }
}
