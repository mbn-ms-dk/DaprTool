using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace demos.Models
{
    public class SecretJsonState
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }
    }
}
