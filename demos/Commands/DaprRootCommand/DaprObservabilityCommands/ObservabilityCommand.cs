using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.OperationalInsights;
using Azure.ResourceManager.Resources;
using demos.Helpers;
using demos.Models;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using YamlDotNet.Serialization;

namespace demos.Commands.DaprRootCommand.DaprObservabilityCommands;

public class ObservabilityCommand : Command
{
    public ObservabilityCommand() : base("obs", "Dapr Observability Demo")
    {
        var deployOption = new Option<bool>(
           name: "--deploy", description: "Create Azure environment (servicebus,eventhub,storage,Azure SQL)");
        AddOption(deployOption);

        var demoOption = new Option<bool>(
            name: "--azure", description: "Use this option to show demo with Azure resources. Without any options the demo will run locally");
        demoOption.AddAlias("-a");
        AddOption(demoOption);

        var deleteOption = new Option<bool>(
            name: "--delete", "Deletes resources used with this demo");
        AddOption(deleteOption);

        var descriptionOption = new Option<bool>(
           name: "--describe", description: "Show a description of the demo");
        AddOption(descriptionOption);

        var tyeOption = new Option<bool>(
            name: "--useTye", description: "Use Tye for demo");
        AddOption(tyeOption);

        this.SetHandler(async (deploy, demo, delete, describe, useTye) =>
        { await Execute(deploy, demo, delete, describe, useTye); }, deployOption, demoOption, deleteOption, descriptionOption, tyeOption);
    }

    private async Task Execute(bool deploy, bool demo, bool delete, bool describe, bool useTye)
    {
        var rgName = "dapr_observability_demo";
        try
        {
            SubscriptionResource? sub = null;
            if (deploy)
            {
                AnsiConsole.MarkupLine("[blue]Deploying to Azure[/]");
                var setting = await Utils.LoadConfiguration();
                sub = await AzureHelpers.GetSubscriptionBasedOnSettings(setting);
                var rg = await AzureHelpers.CreateResourceGroup(sub, rgName);
                await CreateAzureResources(rg, sub, setting);
            }
            else if (delete)
                await AzureHelpers.DeleteResourceGroup(rgName);
            else if (describe)
                Utils.ShowDemoDescription(DaprType.Observability);
            else
            {
                if (demo && !deploy)
                {
                    AnsiConsole.MarkupLine("[blue]Running demo in Azure[/]");
                    //if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/components/pubsub/azure/local_secrets.json"))
                    //    await Execute(true, demo, delete, describe, useTye);
                    await StartDemo("azure", useTye);
                }
                else
                {
                    AnsiConsole.MarkupLine("[green]Running demo local[/]");
                    await StartDemo("local", useTye);
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.Default);
        }
    }

    private static async Task StartDemo(string env, bool useTye)
    {
        if (useTye)
        {
            var cmd = "";
            AnsiConsole.MarkupInterpolated($"[green]Making sure Microsoft.Tye is installed...[/]");
            cmd = "dotnet tool install -g Microsoft.Tye --version \"0.12.0-*\" --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json";
            await Utils.RunCmd(cmd);
            if (env == "local")
            {
                AnsiConsole.MarkupLine("[green]Starting dapr_zipkin container[/]");
                cmd = "docker start dapr_zipkin";
                await Utils.RunCmd(cmd);
                AnsiConsole.MarkupLine("[green]dapr_zipkin container started[/]");
                cmd = $"wt -w 0 sp cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & tye run ./components/obs/tye_local.yaml\"";
                await Utils.RunCmd(cmd);
            }
            else
            {
                AnsiConsole.MarkupLine("[green]Stopping dapr_zipkin container[/]");
                cmd = "docker stop dapr_zipkin";
                await Utils.RunCmd(cmd);
                AnsiConsole.MarkupLine("[green]dapr_zipkin container stopped[/]");
                cmd = $"wt -w 0 sp cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & tye run ./components/obs/tye_cloud.yaml\"";
                await Utils.RunCmd(cmd);
            }
        }
        else
        {
            if (env == "local")
            {
                AnsiConsole.MarkupLine("[green]Stopping otel-opentelemetry container[/]");
                var docker = "docker stop otel-opentelemetry";
                await Utils.RunCmd(docker);
                AnsiConsole.MarkupLine("[green]Stopping otel-opentelemetry container[/]");
                AnsiConsole.MarkupLine("[green]Starting dapr_zipkin container[/]");
                docker = "docker start dapr_zipkin";
                await Utils.RunCmd(docker);
                AnsiConsole.MarkupLine("[green]dapr_zipkin container started[/]");
                Thread.Sleep(500);
            }
            else
            {
                AnsiConsole.MarkupLine("[green]Stopping dapr_zipkin container[/]");
                var docker = "docker stop dapr_zipkin";
                await Utils.RunCmd(docker);
                AnsiConsole.MarkupLine("[green]dapr_zipkin container stopped[/]");
                AnsiConsole.MarkupLine("[green]Pulling otel/opentelemetry-collector-contrib[/]");
                docker = "docker pull otel/opentelemetry-collector-contrib-dev";
                await Utils.RunCmd(docker);
                AnsiConsole.MarkupLine("[green]Pulled otel/opentelemetry-collector-contrib[/]");
                AnsiConsole.MarkupLine("[green]Starting otel/opentelemetry-collector-contrib[/]");
                docker = $"docker run -itd --name otel-opentelemetry -v {AppDomain.CurrentDomain.BaseDirectory}components/obs/config/azure/otel-local-config.yaml:/etc/otel/config.yaml -p 9411:9411 otel/opentelemetry-collector";
                await Utils.RunCmd(docker);
                AnsiConsole.MarkupLine("[green]Started otel/opentelemetry-collector-contrib[/]");
                Thread.Sleep(500);
            }

            var cmdDapr1 = $"dapr run -a serviceA -p 5000 -H 3500 -- dotnet serviceA.dll --urls \"http://localhost:5000\"";
            var cmdDapr2 = $"dapr run -a serviceB -p 5010 -- dotnet serviceB.dll --urls \"http://localhost:5010\"";
            var cmdDapr3 = $"dapr run -a serviceC -p 5020 -- dotnet serviceA.dll --urls \"http://localhost:5020\"";

            var cmd = $"wt -w 0 sp cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & {cmdDapr1}\"";
            cmd += $" ; wt -w 0 sp cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & {cmdDapr2}\"";
            cmd += $" ; wt -w 0 sp -H cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & {cmdDapr3}\"";
            // wt -w 0 sp cmd ; wt -w 0 split-pane -H cmd ;wt -w 0 split-pane -H cmd ;
            await Utils.RunDemo(env, cmd);
        }
    }

    private async Task CreateAzureResources(ResourceGroupResource rg, SubscriptionResource sub, Models.Settings settings)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Creating Azure resources...", async ctx =>
            {
                AnsiConsole.MarkupLine("[green]Creating workspace[/]");
                var ws = await rg.GetOperationalInsightsWorkspaces().CreateOrUpdateAsync(
                    Azure.WaitUntil.Completed,
                    $"ws{Utils.GenerateRandomString(5)}",
                    new OperationalInsightsWorkspaceData(AzureLocation.WestEurope));
                AnsiConsole.MarkupLineInterpolated($"[green]Created workspace:[/] [blue]{ws.Value.Data.Name}[/]");

                AnsiConsole.MarkupLine("[green]Creating application insight[/]");
                var instrKey = await CreateApplicationInsights(rg, ws);
                AnsiConsole.MarkupLine($"[green]Created application insight with key[/] [blue]{instrKey.InstrumentationKey}[/]");

                AnsiConsole.MarkupLine("[green]Writing key to config[/]");
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}/components/obs/config/azure/otel-local-config.yaml";
                var obsStr = await File.ReadAllTextAsync(path);
                var index = obsStr.IndexOf("instrumentation_key:");
                var subReplace = obsStr.Substring(index, Guid.NewGuid().ToString().Length + 21);
                var subOri = subReplace.Substring(21, Guid.NewGuid().ToString().Length);
                var sub = subOri;
                sub = instrKey.InstrumentationKey;
                var nstr = subReplace.Replace(subOri, sub);
                obsStr = obsStr.Replace(subReplace, nstr);
                await File.WriteAllTextAsync($"{AppDomain.CurrentDomain.BaseDirectory}components/obs/config/azure/otel-local-config.yaml", obsStr);
                AnsiConsole.MarkupLine("[green]Key written to config[/]");
            });
    }

    private async Task<AppI> CreateApplicationInsights(ResourceGroupResource rg, ArmOperation<OperationalInsightsWorkspaceResource> ws)
    {
        var result = string.Empty;
        try
        {
            var cmd = $"az monitor app-insights component create --app appi{Helpers.Utils.GenerateRandomString(5)} --location westeurope --kind web -g {rg.Data.Name} --workspace \"{ws.Value.Data.Id}\"";
            var procStartInfo = new ProcessStartInfo("cmd")
            {
                Arguments = $"/c {cmd}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            using var proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.EnableRaisingEvents = true;
            proc.Start();
            result = await proc.StandardOutput.ReadToEndAsync();
            AnsiConsole.MarkupLine($"[blue]{result}[/]");
            await proc.WaitForExitAsync();
            proc.Dispose();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
        return JsonSerializer.Deserialize<AppI>(result);
    }
}
