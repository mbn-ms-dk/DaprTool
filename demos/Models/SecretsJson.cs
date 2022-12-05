using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace demos.Models
{
    public  class SecretsJson
    {
        [property: JsonPropertyName("appId")]
        public string AppId { get; set; }
        [property: JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
        [property: JsonPropertyName("password")]
        public string? Password { get; set; }
        [property: JsonPropertyName("tenant")]
        public string? Tenant { get; set; }

        [property: JsonPropertyName("objectId")]
        public string? ObjectId { get; set; }
        [property: JsonPropertyName("keyvaultName")]
        public string? KeyvaultName { get; set; }
    }
}
