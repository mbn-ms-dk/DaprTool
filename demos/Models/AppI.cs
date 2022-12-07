using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace demos.Models
{
    public class AppI
    {
        [JsonPropertyName("instrumentationKey")]
        public string InstrumentationKey { get; set; }
    }

}
