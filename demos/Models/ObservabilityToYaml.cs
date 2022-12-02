using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace demos.Models
{
    public partial class ObservabilityToYaml
    {
        [JsonPropertyName("receivers")]
        public Receivers Receivers { get; set; }

        [JsonPropertyName("extensions")]
        public Extensions Extensions { get; set; }

        [JsonPropertyName("exporters")]
        public Exporters Exporters { get; set; }

        [JsonPropertyName("service")]
        public Service Service { get; set; }
    }

    public partial class Exporters
    {
        [JsonPropertyName("logging")]
        public Logging Logging { get; set; }

        [JsonPropertyName("azuremonitor")]
        public Azuremonitor Azuremonitor { get; set; }
    }

    public partial class Azuremonitor
    {
        [JsonPropertyName("instrumentation_key")]
        public Guid InstrumentationKey { get; set; }

        [JsonPropertyName("maxbatchinterval")]
        public string Maxbatchinterval { get; set; }

        [JsonPropertyName("maxbatchsize")]
        public long Maxbatchsize { get; set; }
    }

    public partial class Logging
    {
        [JsonPropertyName("loglevel")]
        public string Loglevel { get; set; }
    }

    public partial class Extensions
    {
        [JsonPropertyName("health_check")]
        public object HealthCheck { get; set; }

        [JsonPropertyName("pprof")]
        public Pprof Pprof { get; set; }

        [JsonPropertyName("zpages")]
        public Pprof Zpages { get; set; }
    }

    public partial class Pprof
    {
        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; }
    }

    public partial class Receivers
    {
        [JsonPropertyName("zipkin")]
        public object Zipkin { get; set; }
    }

    public partial class Service
    {
        [JsonPropertyName("extensions")]
        public string[] Extensions { get; set; }

        [JsonPropertyName("pipelines")]
        public Pipelines Pipelines { get; set; }
    }

    public partial class Pipelines
    {
        [JsonPropertyName("traces")]
        public Traces Traces { get; set; }
    }

    public partial class Traces
    {
        [JsonPropertyName("receivers")]
        public string[] Receivers { get; set; }

        [JsonPropertyName("exporters")]
        public string[] Exporters { get; set; }
    }
}
