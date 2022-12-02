using Azure.Core;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.KeyVault.Models;
using Azure.ResourceManager.Resources;
using Spectre.Console;
using System.CommandLine;

namespace demos.Commands.DaprRootCommand.DaprSecretsCommands
{
    public class DaprSecretsCommand : Command
    {
        public DaprSecretsCommand() : base("secrets", "Dapr Secrets Demo")
        {
            var deployOption = new Option<bool>(
               name: "--deploy", description: "Create Azure environment (Keyvault)");
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
            var rgName = "dapr_secrets_demo";
            try
            {
                SubscriptionResource? sub = null;
                if (deploy)
                {
                    AnsiConsole.MarkupLine("[blue]Deploying to Azure[/]");
                    var setting = await Helpers.Utils.LoadConfiguration();
                    sub = await Helpers.AzureHelpers.GetSubscriptionBasedOnSettings(setting);
                    var rg = await Helpers.AzureHelpers.CreateResourceGroup(sub, rgName);
                    await CreateAzureResources(rg, sub);
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

        private async Task CreateAzureResources(ResourceGroupResource rg, SubscriptionResource sub)
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("blue"))
                .StartAsync("Creating Azure resources...", async ctx =>
                {
                    AnsiConsole.MarkupLine("[green]Creating Keyvault[/]");
                    var kvProps = new KeyVaultProperties(
                                sub.Data.TenantId.Value,
                                new KeyVaultSku(
                                    new KeyVaultSkuFamily("A"),
                                    KeyVaultSkuName.Standard))
                    {
                        EnabledForDeployment = true,
                        EnabledForTemplateDeployment = true,
                        EnabledForDiskEncryption = true,
                        EnableSoftDelete = false,
                    };
                    var kvName = $"kv{Helpers.Utils.GenerateRandomString(5)}";
                    var identityPermissions = new IdentityAccessPermissions();
                    identityPermissions.Keys.Add(new IdentityAccessKeyPermission("get"));
                    identityPermissions.Secrets.Add(new IdentityAccessSecretPermission("list"));
                    identityPermissions.Secrets.Add(new IdentityAccessSecretPermission("set"));
                    identityPermissions.Secrets.Add(new IdentityAccessSecretPermission("get"));
                    kvProps.AccessPolicies.Add(new KeyVaultAccessPolicy(
                                sub.Data.TenantId.Value,
                                sub.Data.SubscriptionId,
                                identityPermissions));
                    var kv = await rg.GetKeyVaults().CreateOrUpdateAsync(
                        Azure.WaitUntil.Completed,
                        kvName,
                        new KeyVaultCreateOrUpdateContent(
                            AzureLocation.WestEurope,
                            kvProps));
                    AnsiConsole.MarkupLineInterpolated($"[green]Created keyvault:[/] [blue]{kv.Value.Data.Name}[/]");

                    AnsiConsole.MarkupLine("[green]Creating Keyvault secret[/]");
                  
                    //var secr = await kv.Value.Data.
                    //AnsiConsole.MarkupLineInterpolated($"[green]Created keyvault:[/] [blue]{kv.Value.Data.Name}[/]");
                });
        }
    }
}
