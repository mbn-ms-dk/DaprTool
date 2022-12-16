using System.Text.Json.Serialization;

namespace demos.Models;

public record Settings
{
    [JsonPropertyName("isCustomTenant")]
    public bool CustomTenant { get; set; }

    [JsonPropertyName("customTenantId")]
    public string? CustomTenantId { get; set; }
}
