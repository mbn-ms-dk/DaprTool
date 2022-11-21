using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace demos.Models
{
    public class SecretJsonBinding
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("acct")]
        public string Account { get; set; }
    }
}
