using Azure.Core;
using Azure.ResourceManager.OperationalInsights;
using Azure.ResourceManager.ApplicationInsights;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using demos.Helpers;
using Spectre.Console;
using System.CommandLine;
using Azure.ResourceManager;
using Azure.Identity;
using Azure.Core.Pipeline;

namespace demos.Commands.DaprRootCommand.DaprObservabilityCommands
{
    public class DaprObservabilityCommand : Command
    {
        public DaprObservabilityCommand():base("obs", "Dapr Observability Demo")
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

            this.SetHandler(async (deploy, demo, delete, describe) =>
            { await Execute(deploy, demo, delete, describe); }, deployOption, demoOption, deleteOption, descriptionOption);
        }

        private async Task Execute(bool deploy, bool demo, bool delete, bool describe)
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
                    await CreateAzureResources(rg);
                }
                else if (delete)
                    await AzureHelpers.DeleteResourceGroup(rgName);
                else if (describe)
                    Utils.ShowDemoDescription(DaprType.Pubsub);
                else
                {
                    if (demo && !deploy)
                    {
                        AnsiConsole.MarkupLine("[blue]Running demo in Azure[/]");
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/components/pubsub/azure/local_secrets.json"))
                            await Execute(true, demo, delete, describe);
                        //await StartDemo("azure");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]Running demo local[/]");
                        //await StartDemo("local");
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.Default);
            }
        }

        private async Task CreateAzureResources(ResourceGroupResource rg)
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
                });
        }
    }
}
