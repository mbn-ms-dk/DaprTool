using System.Collections;
using System.Text.Json.Serialization;

namespace demos.Models
{
    public record Settings
    {
        [property: JsonPropertyName("isCustomTenant")]
        public bool CustomTenant { get; set; }
        [property: JsonPropertyName("customTenantId")]
        public string? CustomTenantId { get; set; }
    }
   
}
