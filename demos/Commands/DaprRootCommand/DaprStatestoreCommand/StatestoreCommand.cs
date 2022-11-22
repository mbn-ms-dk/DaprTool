using Azure.Core;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Azure.ResourceManager.Resources;
using demos.Models;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;

namespace demos.Commands.DaprRootCommand.DaprStatestoreCommand
{
    public class StatestoreCommand : Command
    {
        public StatestoreCommand() : base("state", "Dapr statestore demo")
        {
            var deployOption = new Option<bool>(
                name: "--deploy", description: "Create Azure environment (CosmosDB)");
            AddOption(deployOption);

            var deleteOption = new Option<bool>(
                name: "--del", "Deletes resources used with this demo");
            AddOption(deleteOption);

            var demoOption = new Option<bool>(
                name: "--azure", description: "Use this option to show demo with Azure resources");
            demoOption.AddAlias("-a");
            AddOption(demoOption);

            this.SetHandler(async (deploy, demo, delete) =>
            { await Demo(deploy, demo, delete); }, deployOption, demoOption, deleteOption);
        }

        private async Task Demo(bool deploy, bool demo, bool delete)
        {
            var rgName = "dapr_state_demo";
            try
            {
                SubscriptionResource sub = null;

                if (deploy)
                {
                    //await AnsiConsole.Status()
                    //    .Spinner(Spinner.Known.Dots2)
                    //    .SpinnerStyle(Style.Parse("blue"))
                    //    .StartAsync("Deploying to Azure...", async ctx =>
                    //       {
                    //           var setting = await Helpers.Utils.LoadConfiguration();
                    //           sub = setting.CustomTenant ? await Helpers.AzureHelpers.Authenticate(setting.CustomTenantId) : await Helpers.AzureHelpers.Authenticate();
                    //           var rg = await Helpers.AzureHelpers.CreateResourceGroup(sub, rgName);
                    //           await CreateCosmosDb(rg);
                    //       });
                    AnsiConsole.MarkupLine("[blue]RDeploying to Azure[/]");
                    var setting = await Helpers.Utils.LoadConfiguration();
                    sub = setting.CustomTenant ? await Helpers.AzureHelpers.Authenticate(setting.CustomTenantId) : await Helpers.AzureHelpers.Authenticate();
                    var rg = await Helpers.AzureHelpers.CreateResourceGroup(sub, rgName);
                    await CreateCosmosDb(rg);
                    AnsiConsole.MarkupLine("[blue]Resources deployed[/]");
                }
                else if (delete)
                {
                    await Helpers.AzureHelpers.DeleteResourceGroup(rgName);
                }
                else
                {
                    if (demo && !deploy)
                    {
                        AnsiConsole.Markup("[blue]Running demo in Azure[/]");
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/components/state/azure/local_secrets.json"))
                            await Demo(true, demo, delete);
                        await StartDemo("azure");
                    }
                    else
                    {
                        AnsiConsole.Markup("[blue]Running demo local[/]");
                        await StartDemo("local");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private Task StartDemo(string env)
        {
            var cmd = $"dapr run --app-id {env} --dapr-http-port 3500 --components-path {AppDomain.CurrentDomain.BaseDirectory}components/state/{env}";
            var procStartInfo = new ProcessStartInfo("cmd", "/K " + cmd);
            procStartInfo.RedirectStandardOutput = false;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;
            using var proc = Process.Start(procStartInfo);
            proc.WaitForExit();
            return Task.CompletedTask;
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
                            AnsiConsole.MarkupLine($"[green]Created {cdba.Value.Data.Name}[/]");

                            AnsiConsole.Markup($"[green]Creating CosmosSQLDB[/]");
                            var cdb = await cdba.Value.GetCosmosDBSqlDatabases().CreateOrUpdateAsync(
                            Azure.WaitUntil.Completed,
                            "StateStore",
                            new CosmosDBSqlDatabaseCreateOrUpdateContent(
                                AzureLocation.WestEurope,
                                new CosmosDBSqlDatabaseResourceInfo("StateStore")));
                            AnsiConsole.MarkupLine($"[green]Created {cdb.Value.Data.Name}[/]");

                            AnsiConsole.Markup($"[green]Creating SQL Container[/]");
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
                            AnsiConsole.Markup($"[green]Created {cdc.Value.Data.Name}[/]");

                            AnsiConsole.Markup($"[green]Getting Cosmos Keys[/]");
                            var k = await cdba.Value.GetKeysAsync();
                            AnsiConsole.Markup($"[green]Creating Creating components/state/azure/local_secrets.json[/]");

                            var options = new JsonSerializerOptions { WriteIndented = true };
                            var json = JsonSerializer.Serialize(new SecretJsonState()
                            {
                                Url = cdba.Value.Data.DocumentEndpoint,
                                Key = k.Value.PrimaryMasterKey
                            }, options);
                            using var sw = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "components/state/azure/local_secrets.json");
                            sw.Write(json);
                            AnsiConsole.Markup($"[green]Created components/state/azure/local_secrets.json[/]");
                        });
        }
    }
}
