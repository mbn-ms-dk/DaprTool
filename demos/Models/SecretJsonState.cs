using System.Text.Json.Serialization;

namespace demos.Models;

public class SecretJsonState
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }
}
