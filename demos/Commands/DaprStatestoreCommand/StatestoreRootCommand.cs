using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Azure.ResourceManager.Resources;
using Dapr.Client;
using demos.Models;
using System.CommandLine;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace demos.Commands.DaprStatestoreCommand
{
    public class StatestoreRootCommand : Command
    {
        public StatestoreRootCommand() : base("state", "Dapr statestore demo")
        {
            var deployOption = new Option<bool>(
                name: "-deployOnly", description: "Create Azure environment (CosmosDB)");
            AddOption(deployOption);

            var deleteOption = new Option<bool>(
                name: "-del", "Deletes resources used with this demo");
            AddOption(deleteOption);

            var demoOption = new Option<bool>(
                name: "-azure", description: "Use this option to show demo with Azure resources");
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
                    Console.WriteLine("Deploying to Azure");
                    sub = await Helpers.AzureHelpers.Authenticate();
                    var rg = await Helpers.AzureHelpers.CreateResourceGroup(sub, rgName);
                    await CreateCosmosDb(rg);
                }
                else if(delete)
                {
                    await Helpers.AzureHelpers.DeleteResourceGroup(rgName);
                }
                else
                {
                    if (demo && !deploy)
                    {
                        Console.WriteLine("Running demo in Azure");
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/components/state/azure/local_secrets.json"))
                            await Demo(true, demo, delete);
                        await StartDemo("azure");
                    }
                    else
                    {
                        Console.WriteLine("Running demo local");
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
            Console.WriteLine($"Creating CosmosDB...");
            var loc = new CosmosDBAccountLocation
            {
                LocationName = AzureLocation.WestEurope
            };
            Console.WriteLine($"Creating CosmosDB Account");

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

            Console.WriteLine($"Created {cdba.Value.Data.Name}");

            Console.WriteLine($"Creating CosmosSQLDB");
            var cdb = await cdba.Value.GetCosmosDBSqlDatabases().CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                "StateStore", 
                new CosmosDBSqlDatabaseCreateOrUpdateContent(
                    AzureLocation.WestEurope, 
                    new CosmosDBSqlDatabaseResourceInfo("StateStore")));
            Console.WriteLine($"Created {cdb.Value.Data.Name}");

            Console.WriteLine($"Creating SQL Container");
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
            Console.WriteLine($"Created {cdc.Value.Data.Name}");

            var k = await cdba.Value.GetKeysAsync();
            Console.WriteLine("Creating Creating components/state/azure/local_secrets.json");

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize<SecretJsonState>(new SecretJsonState() {
                Url = cdba.Value.Data.DocumentEndpoint,
                Key = k.Value.PrimaryMasterKey }, options);
            using var sw = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "components/state/azure/local_secrets.json");
            sw.Write(json);
            Console.WriteLine("Created Creating components/state/azure/local_secrets.json");
        }
    }
}
