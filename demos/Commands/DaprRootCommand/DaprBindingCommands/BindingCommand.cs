﻿using Azure.Core;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using demos.Models;
using Spectre.Console;
using System.CommandLine;
using System.Text.Json;

namespace demos.Commands.DaprRootCommand.DaprBindingCommands;

public class BindingCommand : Command
{
    public BindingCommand() : base("binding", "Dapr binding demo")
    {
        var deployOption = new Option<bool>(name: "--deploy", description: "Create Azure environment (storage account)");
        AddOption(deployOption);

        var demoOption = new Option<bool>(name: "--azure", description: "Use this option to show demo with Azure resources. Without any options the demo will run locally");
        demoOption.AddAlias("-a");
        AddOption(demoOption);

        var deleteOption = new Option<bool>(name: "--delete", description: "Deletes resources used with this demo");
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
        var rgName = "dapr_binding_demo";
        try
        {
            SubscriptionResource? sub = null;

            if (deploy)
            {
                AnsiConsole.MarkupLine("[blue]Deploying to Azure[/]");
                var setting = await Helpers.Utils.LoadConfiguration();
                sub = await Helpers.AzureHelpers.GetSubscriptionBasedOnSettings(setting);
                var rg = await Helpers.AzureHelpers.CreateResourceGroup(sub, rgName);
                await CreateStorageAccount(rg);
            }
            else if (delete)
            {
                await Helpers.AzureHelpers.DeleteResourceGroup(rgName);
            }
            else if (describe)
            {
                Helpers.Utils.ShowDemoDescription(Helpers.DaprType.Binding);
            }
            else
            {
                if (demo && !deploy)
                {
                    AnsiConsole.MarkupLine("[blue]Running demo in Azure[/]");
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/components/binding/azure/local_secrets.json"))
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

    private async Task StartDemo(string env)
    {
        var cmdDapr = $"dapr run --app-id {env} --dapr-http-port 3500 --components-path ./components/binding/{env}";

        var cmd = $"wt -w 0 split-pane cmd /c \"cd {AppDomain.CurrentDomain.BaseDirectory} & {cmdDapr}\"";//$"wt cmd /K {cmdDapr}";

        //if (env == "local")
        //    cmd += "; wt -w 0 sp -H cmd /c start msedge http://localhost:8080; wt -w 0 sp -H cmd /c dapr dashboard";
        await Helpers.Utils.RunDemo(env, cmd);
    }

    private async Task CreateStorageAccount(ResourceGroupResource rg)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots2)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Creating storage account...", async ctx =>
            {
                var stor = await rg.GetStorageAccounts().CreateOrUpdateAsync(
                    Azure.WaitUntil.Completed,
                    $"stg{Helpers.Utils.GenerateRandomString(4)}demo",
                    new StorageAccountCreateOrUpdateContent(
                        new StorageSku(StorageSkuName.StandardLrs),
                        StorageKind.StorageV2,
                        AzureLocation.WestEurope));
                AnsiConsole.MarkupInterpolated($"[green]Created storage account:[/] [blue] {stor.Value.Data.Name}[/]");

                AnsiConsole.MarkupLine("[green]Creating components/binding/azure/local_secrets.json[/]");

                AnsiConsole.Markup($"[green]Getting storageaccount Keys[/]");
                var keys = stor.Value.GetKeys().AsPages();

                var k = keys.FirstOrDefault().Values.FirstOrDefault();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping //to allow '+' signs in key
                };
                var json = JsonSerializer.Serialize(new SecretJsonBinding()
                {
                    Key = k.Value,
                    Account = stor.Value.Data.Name
                }, options);

                await Helpers.Utils.SaveSecretsFile(Helpers.DaprType.Binding, json);
                AnsiConsole.MarkupLine("[green]Created[/] [blue]components/binding/azure/local_secrets.json[/]");
            });
    }
}
