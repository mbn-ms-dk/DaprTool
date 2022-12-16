using System.Text.Json.Serialization;

namespace demos.Models;

public  class MyIp
{
    [JsonPropertyName("ip")]
    public string MyIpAddress { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("cc")]
    public string? CountryCode { get; set; }
}
