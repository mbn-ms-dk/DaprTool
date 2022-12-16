using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.KeyVault.Models;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using demos.Models;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;

namespace demos.Commands.DaprRootCommand.DaprSecretsCommands;

public class SecretsCommand : Command
{
    public SecretsCommand() : base("secrets", "Dapr Secrets Demo")
    {
        var deployOption = new Option<bool>(name: "--deploy", description: "Create Azure environment (Keyvault, service principal)");
        AddOption(deployOption);

        var demoOption = new Option<bool>(name: "--azure", description: "Use this option to show demo with Azure resources. Without any options the demo will run locally");
        demoOption.AddAlias("-a");
        AddOption(demoOption);

        var deleteOption = new Option<bool>(name: "--delete", "Deletes resources used with this demo");
        AddOption(deleteOption);

        var descriptionOption = new Option<bool>(name: "--describe", description: "Show a description of the demo");
        AddOption(descriptionOption);

        this.SetHandler(async (deploy, demo, delete, describe) =>
        {
            await Execute(deploy, demo, delete, describe);
        }, deployOption, demoOption, deleteOption, descriptionOption);
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
                await CreateAzureResources(rg, sub, setting);
            }
            else if (delete)
                await Helpers.AzureHelpers.DeleteResourceGroup(rgName);
            else if (describe)
                Helpers.Utils.ShowDemoDescription(Helpers.DaprType.Secrets);
            else
            {
                if (demo && !deploy)
                {
                    AnsiConsole.MarkupLine("[blue]Running demo in Azure[/]");
                    var secretSettings = await Helpers.Utils.LoadSecretsFile(Helpers.DaprType.Secrets);
                    var ss = JsonSerializer.Deserialize<SecretsJson>(secretSettings);
                    if (ss == null || string.IsNullOrEmpty(ss.AppId))
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
            AnsiConsole.WriteException(ex, ExceptionFormats.Default);
        }
    }

    private static async Task StartDemo(string env)
    {
        var cmdDapr = $"dapr run --app-id {env} --dapr-http-port 3500 --components-path ./components/secrets/{env}";

        var cmd = $"wt -w 0 split-pane cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & {cmdDapr}\"";//$"wt cmd /K {cmdDapr}";

        // wt -w 0 sp cmd ; wt -w 0 split-pane -H cmd ;wt -w 0 split-pane -H cmd ;
        await Helpers.Utils.RunDemo(env, cmd);
    }

    private static async Task<SecretsJson> CreateServicePrincipal(ResourceGroupResource rg, string spName)
    {
        var cmd = string.Empty;
        var result = string.Empty;
        var secretsJson = new SecretsJson();
        try
        {
            AnsiConsole.MarkupLineInterpolated($"[green]Creating serviceprincipal[/]");
            cmd = $"az ad sp create-for-rbac -n {spName} --role Contributor --scopes {rg.Id}";

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
            AnsiConsole.MarkupLineInterpolated($"[green]Created serviceprincipal[/]");
            secretsJson = JsonSerializer.Deserialize<SecretsJson>(result);
            AnsiConsole.MarkupLineInterpolated($"[green]Getting serviceprincipal object id[/]");
            cmd = $"az ad sp show --id {secretsJson.AppId} --query id --output tsv";
            procStartInfo = new ProcessStartInfo("cmd")
            {
                Arguments = $"/c {cmd}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            using var proc1 = new Process();
            proc1.StartInfo = procStartInfo;
            proc1.EnableRaisingEvents = true;
            proc1.Start();
            result = await proc1.StandardOutput.ReadToEndAsync();
            secretsJson.ObjectId = result.Trim();
            AnsiConsole.MarkupLine($"[blue]{result}[/]");
            await proc1.WaitForExitAsync();
            proc1.Dispose();
            AnsiConsole.MarkupLineInterpolated($"[green]Retrieved serviceprincipal object id[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
        return secretsJson;
    }

    private async Task CreateAzureResources(ResourceGroupResource rg, SubscriptionResource sub, Settings settings)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Creating Azure resources...", async ctx =>
            {
                var kvName = $"kv{Helpers.Utils.GenerateRandomString(5)}";
                var secretsJson = await CreateServicePrincipal(rg, $"app{kvName}");

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

                var identityPermissions = new IdentityAccessPermissions();
                identityPermissions.Keys.Add(new IdentityAccessKeyPermission("get"));
                identityPermissions.Secrets.Add(new IdentityAccessSecretPermission("list"));
                identityPermissions.Secrets.Add(new IdentityAccessSecretPermission("set"));
                identityPermissions.Secrets.Add(new IdentityAccessSecretPermission("get"));
                kvProps.AccessPolicies.Add(new KeyVaultAccessPolicy(
                            sub.Data.TenantId.Value,
                            secretsJson.ObjectId,
                            identityPermissions));
                var kv = await rg.GetKeyVaults().CreateOrUpdateAsync(
                    Azure.WaitUntil.Completed,
                    kvName,
                    new KeyVaultCreateOrUpdateContent(
                        AzureLocation.WestEurope,
                        kvProps));
                AnsiConsole.MarkupLineInterpolated($"[green]Created keyvault:[/] [blue]{kv.Value.Data.Name}[/]");
                AnsiConsole.MarkupLine("[green]saving settings[/]");
                secretsJson.KeyVaultName = kvName;
                await Helpers.Utils.SaveSecretsFile(Helpers.DaprType.Secrets, JsonSerializer.Serialize<SecretsJson>(secretsJson));
                AnsiConsole.MarkupLineInterpolated($"[green]Saved settings[/]");
                AnsiConsole.MarkupLine("[green]Creating Keyvault secret Client[/]");
                var cc = new ClientSecretCredential(secretsJson.Tenant, secretsJson.AppId, secretsJson.Password);
                var secr = new SecretClient(kv.Value.Data.Properties.VaultUri, cc);
                AnsiConsole.MarkupLineInterpolated($"[green]Created keyvault secret Client[/]");

                AnsiConsole.MarkupLine("[green]Creating Keyvault secret [/]");
                var secretName = $"my-secret";
                var secret = new KeyVaultSecret(secretName, "My_Secret_From_Azure_KeyVault");
                await secr.SetSecretAsync(secret);
                AnsiConsole.MarkupLine("[green]Created Keyvault secret [/]");
            });
    }
}
