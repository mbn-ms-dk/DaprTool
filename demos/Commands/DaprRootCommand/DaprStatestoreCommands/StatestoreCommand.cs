using Azure.Core;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Azure.ResourceManager.Resources;
using demos.Models;
using Spectre.Console;
using System.CommandLine;
using System.Text.Json;

namespace demos.Commands.DaprRootCommand.DaprStatestoreCommands;

public class StatestoreCommand : Command
{
    public StatestoreCommand() : base("state", "Dapr statestore demo")
    {
        var deployOption = new Option<bool>(name: "--deploy", description: "Create Azure environment (CosmosDB)");
        AddOption(deployOption);

        var deleteOption = new Option<bool>(name: "--delete", "Deletes resources used with this demo");
        AddOption(deleteOption);

        var demoOption = new Option<bool>(name: "--azure", description: "Use this option to show demo with Azure resources. Without any options the demo will run locally");
        demoOption.AddAlias("-a");
        AddOption(demoOption);

        var descriptionOption = new Option<bool>(name: "--describe", description: "Show a description of the demo");
        AddOption(descriptionOption);

        this.SetHandler(async (deploy, demo, delete, describe) =>
        {
            await Execute(deploy, demo, delete, describe);
        }, deployOption, demoOption, deleteOption, descriptionOption);
    }

    private async Task Execute(bool deploy, bool demo, bool delete, bool describe)
    {
        var rgName = "dapr_state_demo";
        try
        {
            SubscriptionResource sub = null;

            if (deploy)
            {
                AnsiConsole.MarkupLine("[blue]RDeploying to Azure[/]");
                var setting = await Helpers.Utils.LoadConfiguration();
                sub = await Helpers.AzureHelpers.GetSubscriptionBasedOnSettings(setting);
                var rg = await Helpers.AzureHelpers.CreateResourceGroup(sub, rgName);
                await CreateCosmosDb(rg);
                AnsiConsole.MarkupLine("[blue]Resources deployed[/]");
            }
            else if (delete)
                await Helpers.AzureHelpers.DeleteResourceGroup(rgName);
            else if (describe)
                Helpers.Utils.ShowDemoDescription(Helpers.DaprType.State);
            else
            {
                if (demo && !deploy)
                {
                    AnsiConsole.Markup("[blue]Running demo in Azure[/]");
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/components/state/azure/local_secrets.json"))
                        await Execute(true, demo, delete, describe);
                    await StartDemo("azure");
                }
                else
                {
                    AnsiConsole.MarkupLine("[green]Running demo local[/]");
                    await StartDemo("local");
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    private async Task StartDemo(string env)
    {
        var cmdDapr = $"dapr run --app-id {env} --dapr-http-port 3500 --components-path ./components/state/{env}";

        var cmd = $"wt -w 0 split-pane cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & {cmdDapr}\"";

        await Helpers.Utils.RunDemo(env, cmd);
    }

    private async Task CreateCosmosDb(ResourceGroupResource rgr)
    {
        await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots2)
                    .SpinnerStyle(Style.Parse("blue"))
                    .StartAsync("Creating CosmosDB...", async ctx =>
                    {
                        var loc = new CosmosDBAccountLocation
                        {
                            LocationName = AzureLocation.WestEurope
                        };
                        AnsiConsole.Markup($"[green]Creating CosmosDB Account[/]");
                        var cdbAccContent = new CosmosDBAccountCreateOrUpdateContent(
                        AzureLocation.WestEurope,
                        new List<CosmosDBAccountLocation> { loc })
                        {
                            ConsistencyPolicy = new ConsistencyPolicy(DefaultConsistencyLevel.Session),
                            Location = AzureLocation.WestEurope,
                            EnableAutomaticFailover = false,
                            DatabaseAccountOfferType = CosmosDBAccountOfferType.Standard,
                            Kind = CosmosDBAccountKind.GlobalDocumentDB
                        };
                        cdbAccContent.Capabilities.Add(new CosmosDBAccountCapability() { Name = "EnableServerless" });

                        var cdba = await rgr.GetCosmosDBAccounts().CreateOrUpdateAsync(
                            Azure.WaitUntil.Completed,
                            $"cdb{Helpers.Utils.GenerateRandomString(5)}demo",
                            cdbAccContent);
                        AnsiConsole.MarkupLineInterpolated($"[green]Created CosmosDB Account:[/] [blue]{cdba.Value.Data.Name}[/]");

                        AnsiConsole.MarkupLine($"[green]Creating CosmosSQLDB[/]");
                        var cdb = await cdba.Value.GetCosmosDBSqlDatabases().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        "StateStore",
                        new CosmosDBSqlDatabaseCreateOrUpdateContent(
                            AzureLocation.WestEurope,
                            new CosmosDBSqlDatabaseResourceInfo("StateStore")));
                        AnsiConsole.MarkupLineInterpolated($"[green]Created CosmosDBSQL:[/] [blue]{cdb.Value.Data.Name}[/]");

                        AnsiConsole.MarkupLine($"[green]Creating SQL Container[/]");
                        var key = new CosmosDBContainerPartitionKey();
                        key.Paths.Add("/id");
                        var cdc = await cdb.Value.GetCosmosDBSqlContainers().CreateOrUpdateAsync(
                            Azure.WaitUntil.Completed,
                            "StateStoreValues",
                            new CosmosDBSqlContainerCreateOrUpdateContent(
                                AzureLocation.WestEurope,
                                new CosmosDBSqlContainerResourceInfo("StateStoreValues")
                                {
                                    PartitionKey = key
                                }));
                        AnsiConsole.MarkupLineInterpolated($"[green]Created SQL Container:[/] [blue]{cdc.Value.Data.Name}[/]");

                        AnsiConsole.MarkupLine($"[green]Getting Cosmos Keys[/]");
                        var k = await cdba.Value.GetKeysAsync();
                        AnsiConsole.MarkupLine($"[green]Creating components/state/azure/local_secrets.json[/]");

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping //to allow '+' signs in key
                        };
                        var json = JsonSerializer.Serialize(new SecretJsonState()
                        {
                            Url = cdba.Value.Data.DocumentEndpoint,
                            Key = k.Value.PrimaryMasterKey
                        }, options);
                        await Helpers.Utils.SaveSecretsFile(Helpers.DaprType.State, json);
                        AnsiConsole.MarkupLine($"[green]Created components/state/azure/local_secrets.json[/]");
                    });
    }
}
