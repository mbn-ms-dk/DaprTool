using YamlDotNet.Serialization;

namespace demos.Models;

public class ObservabilityToYaml
{
    [YamlMember(Alias = "receivers")]
    public Receivers Receivers { get; set; }

    [YamlMember(Alias = "extensions")]
    public Extensions Extensions { get; set; }

    [YamlMember(Alias = "exporters")]
    public Exporters Exporters { get; set; }

    [YamlMember(Alias = "service")]
    public Service Service { get; set; }
}

#region receivers
public class Receivers
{
    [YamlMember(Alias = "zipkin")]
    public Zipkin Zipkin { get; set; }
}

public class Zipkin { }
#endregion receivers

#region extensions
public class Extensions
{
    [YamlMember(Alias = "pprof")]
    public Pprof Pprof { get; set; }

    [YamlMember(Alias = "zpages")]
    public Zpages Zpages { get; set; }
}

public class HealthCheck { }

public class Pprof
{
    [YamlMember(Alias = "endpoint")]
    public long Endpoint { get; set; } = 1888;
}

public class Zpages
{
    [YamlMember(Alias = "endpoint")]
    public long Endpoint { get; set; } = 55679;
}
#endregion

#region exporters
public class Exporters
{
    [YamlMember(Alias = "logging")]
    public Logging Logging { get; set; }

    [YamlMember(Alias = "azuremonitor")]
    public AzureMonitor AzureMonitor { get; set; }
}

public class Logging
{
    [YamlMember(Alias = "logLevel")]
    public string LogLevel { get; set; } = "debug";
}

public class AzureMonitor
{
    [YamlMember(Alias = "instrumentation_key")]
    public string InstrumentationKey { get; set; }

    [YamlMember(Alias = "maxbatchinterval")]
    public string MaxBatchInterval { get; set; } = "5s";

    [YamlMember(Alias = "maxbatchsize")]
    public int MaxBatchSize { get; set; } = 5;
}
#endregion

#region service
public class Service
{
    [YamlMember(Alias = "extensions")]
    public ServiceExtensions Extensions { get; set; }

    [YamlMember(Alias = "pipelines")]
    public Pipelines Pipelines { get; set; }
}

public class ServiceExtensions
{
    public string[] Extensions { get; set; }
}

public class Pipelines
{
    [YamlMember(Alias = "traces")]
    public Traces Traces { get; set; }
}

public class Traces
{
    [YamlMember(Alias = "receivers")]
    public ServicePipelineTraceReceivers Receivers { get; set; }
    
    [YamlMember(Alias = "exporters")]
    public ServicePipelineTraceExporters Exporters { get; set; }
}

public class ServicePipelineTraceReceivers
{
    public string[] Receivers { get; set; }
}

public class ServicePipelineTraceExporters
{
    public string[] Exporters { get; set; }
}
#endregion
