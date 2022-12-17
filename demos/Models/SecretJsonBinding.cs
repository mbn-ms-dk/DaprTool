using System.Text.Json.Serialization;

namespace demos.Models;

public class SecretJsonBinding
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("acct")]
    public string Account { get; set; }
}
