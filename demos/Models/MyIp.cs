using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace demos.Models
{
    public  class MyIp
    {
        [property: JsonPropertyName("ip")]
        public string MyIpAddress { get; set; }
        [property: JsonPropertyName("country")]
        public string? Country { get; set; }
        [property: JsonPropertyName("cc")]
        public string? CountryCode { get; set; }
    }
}
