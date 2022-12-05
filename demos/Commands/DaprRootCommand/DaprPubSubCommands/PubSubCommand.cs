using Azure.Core;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.EventHubs.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.ServiceBus;
using Azure.ResourceManager.ServiceBus.Models;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using demos.Models;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;

namespace demos.Commands.DaprRootCommand.DaprPubSubCommands
{
    public class PubSubCommand : Command
    {
        public PubSubCommand() : base("pubsub", "Dapr pubsub demo")
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
            var rgName = "dapr_pubsub_demo";
            try
            {
                SubscriptionResource? sub = null;
                if (deploy)
                {
                    AnsiConsole.MarkupLine("[blue]Deploying to Azure[/]");
                    var setting = await Helpers.Utils.LoadConfiguration();
                    sub = await Helpers.AzureHelpers.GetSubscriptionBasedOnSettings(setting);
                    var rg = await Helpers.AzureHelpers.CreateResourceGroup(sub, rgName);
                    await CreateAzureResources(rg);
                }
                else if (delete)
                    await Helpers.AzureHelpers.DeleteResourceGroup(rgName);
                else if (describe)
                    Helpers.Utils.ShowDemoDescription(Helpers.DaprType.Pubsub);
                else
                {
                    if (demo && !deploy)
                    {
                        AnsiConsole.MarkupLine("[blue]Running demo in Azure[/]");
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/components/pubsub/azure/local_secrets.json"))
                            await Execute(true, demo, delete,describe);
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
                AnsiConsole.WriteException(ex, ExceptionFormats.Default);
            }
        }

        private static async Task StartDemo(string env)
        {
            var cmdDapr = $"dapr run --app-id app1 --app-port 5000 --dapr-http-port 3500 --components-path ./components/pubsub/{env} -- dotnet pubsub.dll";

            var cmd = $"wt -w 0 split-pane cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & {cmdDapr}\"";//$"wt cmd /K {cmdDapr}"; 

            // wt -w 0 sp cmd ; wt -w 0 split-pane -H cmd ;wt -w 0 split-pane -H cmd ;

            var procStartInfo = new ProcessStartInfo("cmd")
            {
                Arguments = $"/c {cmd}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.EnableRaisingEvents = true;
            proc.Start();
            await proc.WaitForExitAsync();
            proc.Dispose();
            AnsiConsole.MarkupLineInterpolated($"[yellow]Running Dapr with app-id {env}[/]");
        }

        private async Task CreateAzureResources(ResourceGroupResource rg)
        {
            var dbPsw = "SecPass@Word123";
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("blue"))
                .StartAsync("Creating Azure resources...", async ctx =>
                {
                    AnsiConsole.MarkupLine("[green]Creating servicebus namespace[/]");
                    var sb = await rg.GetServiceBusNamespaces().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        $"sb{Helpers.Utils.GenerateRandomString(5)}",
                        new ServiceBusNamespaceData(AzureLocation.WestEurope)
                        {
                            Sku = new ServiceBusSku(ServiceBusSkuName.Standard)
                            {
                                Tier = ServiceBusSkuTier.Standard
                            }
                        });
                    AnsiConsole.MarkupLineInterpolated($"[green]Created servicebus namespace:[/] [blue]{sb.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating servicebus namespace authorization rule[/]");
                    var sbAuthData = new ServiceBusAuthorizationRuleData();
                    sbAuthData.Rights.Add(ServiceBusAccessRight.Manage);
                    sbAuthData.Rights.Add(ServiceBusAccessRight.Listen);
                    sbAuthData.Rights.Add(ServiceBusAccessRight.Send);
                    var sbAuth = await sb.Value.GetServiceBusNamespaceAuthorizationRules().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        "RootManageSharedAccessKey",
                        sbAuthData);
                    AnsiConsole.MarkupLineInterpolated($"[green]Created servicebus namespace authorization rule:[/] [blue]{sbAuth.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating event hub namespace[/]");
                    var evns = await rg.GetEventHubsNamespaces().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        $"eh{Helpers.Utils.GenerateRandomString(5)}",
                        new EventHubsNamespaceData(AzureLocation.WestEurope));
                    AnsiConsole.MarkupLineInterpolated($"[green]Created event hub namespace[/] [blue]{evns.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating event hub[/]");
                    var eh = await evns.Value.GetEventHubs().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        "neworder",
                        new EventHubData());
                    AnsiConsole.MarkupLineInterpolated($"[green]Created event hub:[/] [blue]{eh.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating consumer group[/]");
                    var cg = await eh.Value.GetEventHubsConsumerGroups().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        "app1",
                        new EventHubsConsumerGroupData());
                    AnsiConsole.MarkupLineInterpolated($"[green]Created consumer group:[/] [blue]{cg.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating event hub authorization rule[/]");

                    var authData = new EventHubsAuthorizationRuleData();
                    authData.Rights.Add(EventHubsAccessRight.Manage);
                    authData.Rights.Add(EventHubsAccessRight.Listen);
                    authData.Rights.Add(EventHubsAccessRight.Send);

                    var ehAuth = await eh.Value.GetEventHubAuthorizationRules().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        "dapr",
                        authData);
                    AnsiConsole.MarkupLineInterpolated($"[green]Created event hub authorization rule:[/] [blue]{ehAuth.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating storage account[/]");
                    var stor = await rg.GetStorageAccounts().CreateOrUpdateAsync(
                       Azure.WaitUntil.Completed,
                       $"stg{Helpers.Utils.GenerateRandomString(5)}",
                       new StorageAccountCreateOrUpdateContent(
                           new StorageSku(StorageSkuName.StandardLrs),
                           StorageKind.Storage,
                           AzureLocation.WestEurope));
                    AnsiConsole.MarkupLineInterpolated($"[green]Created storage account:[/] [blue]{stor.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating Azure SQL[/]");
                    var sql = await rg.GetSqlServers().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        $"sql{Helpers.Utils.GenerateRandomString(5)}",
                        new SqlServerData(AzureLocation.WestEurope)
                        {
                            AdministratorLogin = "dapr",
                            AdministratorLoginPassword = dbPsw
                        });
                    AnsiConsole.MarkupLineInterpolated($"[green]Created azure SQL:[/] [blue]{sql.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Getting public Ip Address to add to SQL firewall[/]");
                    var ipAdress = await Helpers.Utils.GetPublicIpAddress();
                    AnsiConsole.MarkupLineInterpolated($"[green]Received public Ip Address:[/] [blue]{ipAdress}[/]");

                    AnsiConsole.MarkupLine("[green]Creating SQL firewall rule[/]");
                    var sqlFw = await sql.Value.GetSqlFirewallRules().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        "demos tool rule",
                        new SqlFirewallRuleData()
                        {
                            StartIPAddress = ipAdress,
                            EndIPAddress = ipAdress
                        });
                    AnsiConsole.MarkupLineInterpolated($"[green]Created SQL firewall rule:[/] [blue]{sqlFw.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating SQL Database[/]");
                    var db = await sql.Value.GetSqlDatabases().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        "dapr",
                        new SqlDatabaseData(AzureLocation.WestEurope)
                        {
                            Sku = new SqlSku("Standard")
                            {
                                Tier = "Standard"
                            }
                        });
                    AnsiConsole.MarkupLineInterpolated($"[green]Created SQL Database:[/] [blue]{db.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine($"[green]Creating components/pubsub/azure/local_secrets.json[/]");

                    var storageAccountName = stor.Value.Data.Name;
                    var keys = stor.Value.GetKeys().AsPages();
                    var k = keys.FirstOrDefault().Values.FirstOrDefault();
                    var storageAccountKey = k.Value;
                    var serviceBusAuthRule = sbAuth.Value.Data.Name;
                    var serviceBusNamespace = sb.Value.Data.Name;
                    var sbk = await sbAuth.Value.GetKeysAsync();
                    var eventHubName = eh.Value.Data.Name;
                    var eventHubAuthRule = ehAuth.Value.Data.Name;
                    var eventHubsNamespace = evns.Value.Data.Name;
                    var ehk = await ehAuth.Value.GetKeysAsync();
                    var databaseName = "dapr";
                    var administratorLogin = "dapr";
                    var fullyQualifiedDomainName = sql.Value.Data.FullyQualifiedDomainName;

                    var eventHubsEndpoint = ehk.Value.PrimaryConnectionString;
                    var serviceBusEndpoint = sbk.Value.PrimaryConnectionString;

                    var connStr = $"server={fullyQualifiedDomainName};database={databaseName};user id={administratorLogin};Password={dbPsw};port=1433;";

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping //to allow '+' signs in key
                    };

                    var json = JsonSerializer.Serialize<SecretJsonPubsub>(new SecretJsonPubsub()
                    {
                        IPAddress = ipAdress,
                        DatabaseName = databaseName,
                        SqlConnectionString = connStr,
                        EventHubsEndpoint = eventHubsEndpoint,
                        ServiceBusEndpoint = serviceBusEndpoint,
                        StorageAccountName = storageAccountName,
                        StorageAccountKey = storageAccountKey
                    }, options);
                    await Helpers.Utils.SaveSecretsFile(Helpers.DaprType.Pubsub, json);
                    AnsiConsole.MarkupLine($"[green]Created components/pubsub/azure/local_secrets.json[/]");
                });
        }
    }
}
