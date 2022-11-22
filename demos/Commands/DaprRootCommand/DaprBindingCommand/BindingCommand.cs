using Azure.Core;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using demos.Models;
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;

namespace demos.Commands.DaprRootCommand.DaprBindingCommand
{
    public class BindingCommand : Command
    {
        public BindingCommand() : base("binding", "Dapr binding demo")
        {
            var deployOption = new Option<bool>(
               name: "-deployOnly", description: "Create Azure environment (storageaccount)");
            AddOption(deployOption);

            var demoOption = new Option<bool>(
                name: "-azure", description: "Use this option to show demo with Azure resources");
            demoOption.AddAlias("-a");
            AddOption(demoOption);

            var deleteOption = new Option<bool>(
                name: "-del", "Deletes resources used with this demo");
            AddOption(deleteOption);

            this.SetHandler(async (deploy, demo, delete) =>
            { await Demo(deploy, demo, delete); }, deployOption, demoOption, deleteOption);
        }

        private async Task Demo(bool deploy, bool demo, bool delete)
        {
            var rgName = "dapr_binding_demo";
            try
            {
                SubscriptionResource? sub = null;

                if (deploy)
                {
                    Console.WriteLine("Deploying to Azure");
                    var setting = await Helpers.Utils.LoadConfiguration();
                    sub = setting.CustomTenant ? await Helpers.AzureHelpers.Authenticate(setting.CustomTenantId) : await Helpers.AzureHelpers.Authenticate();
                    var rg = await Helpers.AzureHelpers.CreateResourceGroup(sub, rgName);
                    await CreateStorageAccount(rg);
                }
                else if (delete)
                {
                    await Helpers.AzureHelpers.DeleteResourceGroup(rgName);
                }
                else
                {
                    if (demo && !deploy)
                    {
                        Console.WriteLine("Running demo in Azure");
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/components/binding/azure/local_secrets.json"))
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
            var cmd = $"dapr run --app-id {env} --dapr-http-port 3500 --components-path {AppDomain.CurrentDomain.BaseDirectory}components/binding/{env}";
            var procStartInfo = new ProcessStartInfo("cmd", "/K " + cmd);
            procStartInfo.RedirectStandardOutput = false;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;
            using var proc = Process.Start(procStartInfo);
            proc.WaitForExit();
            return Task.CompletedTask;
        }

        private async Task CreateStorageAccount(ResourceGroupResource rg)
        {
            Console.WriteLine("Creating storage account");
            var stor = await rg.GetStorageAccounts().CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                $"stg{Helpers.Utils.GenerateRandomString(4)}demo",
                new StorageAccountCreateOrUpdateContent(
                    new StorageSku(StorageSkuName.StandardLrs),
                    StorageKind.Storage,
                    AzureLocation.WestEurope));
            Console.WriteLine($"Created storage account: {stor.Value.Data.Name}");

            Console.WriteLine("Creating Creating components/binding/azure/local_secrets.json");

            var keys = stor.Value.GetKeys().AsPages();

            var k = keys.FirstOrDefault().Values.FirstOrDefault();
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping; //to allow '+' signs in key
            var json = JsonSerializer.Serialize(new SecretJsonBinding()
            {
                Key = k.Value,
                Account = stor.Value.Data.Name
            }, options);
            using var sw = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "components/binding/azure/local_secrets.json");
            sw.Write(json);
            Console.WriteLine("Created Creating components/binding/azure/local_secrets.json");
        }
    }
}
