using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using demos.Models;
using Spectre.Console;

namespace demos.Helpers;

public static class AzureHelpers
{
    public static async Task<SubscriptionResource> Authenticate()
    {
        SubscriptionResource? sub = null;
        await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Authenticating...", async ctx =>
            {
                AnsiConsole.MarkupLine("[green]Creating Client[/]");
                var client = new ArmClient(new DefaultAzureCredential());
                AnsiConsole.MarkupLine("[green]Client Created[/]");
                AnsiConsole.MarkupLine("[green]Getting Default subscription[/]");
                sub = await client.GetDefaultSubscriptionAsync();
            });
        AnsiConsole.MarkupLineInterpolated($"[green]Default Subscription Name:[/] [blue]{sub?.Data.DisplayName}[/]");
        return sub;
    }

    public static async Task<SubscriptionResource> Authenticate(string tenantId)
    {
        SubscriptionResource sub = default;
        //authenticate
        await AnsiConsole.Status()
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Authenticating...", async ctx =>
            {
                AnsiConsole.MarkupLine("[green]Setting Tenant Id[/]");
                var credential = new AzureCliCredential(new AzureCliCredentialOptions()
                {
                    TenantId = tenantId
                });
                AnsiConsole.MarkupLine("[green]Creating Client[/]");
                var client = new ArmClient(credential);
                AnsiConsole.MarkupLine("[green]Client Created[/]");
                AnsiConsole.MarkupLine("[green]Getting Default subscription[/]");
                sub = await client.GetDefaultSubscriptionAsync();
            });
        AnsiConsole.MarkupLineInterpolated($"[green]Default Subscription Name:[/] [blue]{sub?.Data.DisplayName}[/]");
        return sub!;
    }

    public static async Task<SubscriptionResource> GetSubscriptionBasedOnSettings(Settings settings)
    {
        if (settings.CustomTenant)
            return await Authenticate(settings.CustomTenantId);
        else
            return await Authenticate();
    }

    public static async Task<SubscriptionResource> GetDefaultSubscription()
    {
        var setting = await Utils.LoadConfiguration();
        if (setting.CustomTenant)
            return await Authenticate(setting.CustomTenantId);

        return await Authenticate();
    }

    public static async Task ListSubscriptions()
    {
        var setting = await Utils.LoadConfiguration();
        ArmClient client = null;
        SubscriptionCollection subs = null;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Moon)
            .SpinnerStyle(Style.Parse("blue"))
            .Start("Getting subscriptions", ctx =>
            {

                if (setting.CustomTenant)
                {
                    var credential = new AzureCliCredential(new AzureCliCredentialOptions()
                    {
                        TenantId = setting.CustomTenantId
                    });
                    AnsiConsole.MarkupLine("[green]Creating Client[/]");
                    client = new ArmClient(credential);
                    AnsiConsole.MarkupLine("[green]Client Created[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[green]Creating Client[/]");
                    client = new ArmClient(new DefaultAzureCredential());
                    AnsiConsole.MarkupLine("[green]Client Created[/]");
                }
                AnsiConsole.MarkupLine("[green]Fetching subscriptions[/]");
                subs = client.GetSubscriptions();
            });
        var table = new Table().LeftAligned();
        table.Border(TableBorder.Rounded);
        AnsiConsole.Live(table)
        .Overflow(VerticalOverflow.Ellipsis)
        .Start(ctx =>
        {
            table.AddColumn("[bold]Subscription Name[/]");
            ctx.Refresh();

            table.AddColumn("[bold]Subscription Id[/]");
            ctx.Refresh();

            foreach (var sub in subs)
            {
                table.AddRow(new Markup($"[blue]{sub.Data.DisplayName}[/]"), new Markup($"[green]{sub.Id}[/]"));
                ctx.Refresh();
                Thread.Sleep(250);
            }
        });
    }

    public static async Task ListTenants()
    {
        var setting = await Utils.LoadConfiguration();
        ArmClient client = null;
        TenantCollection? tenants = null;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Moon)
            .SpinnerStyle(Style.Parse("blue"))
            .Start("Retrieving tenants...", ctx =>
            {
                if (setting.CustomTenant)
                {
                    var credential = new AzureCliCredential(new AzureCliCredentialOptions()
                    {
                        TenantId = setting.CustomTenantId
                    });
                    AnsiConsole.MarkupLine("[green]Creating Client[/]");
                    client = new ArmClient(credential);
                    AnsiConsole.MarkupLine("[green]Client Created[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[green]Creating Client[/]");
                    client = new ArmClient(new DefaultAzureCredential());
                    AnsiConsole.MarkupLine("[green]Client Created[/]");
                }
                AnsiConsole.MarkupLine("[green]Getting tenants[/]");
                tenants = client.GetTenants();
            });

        var table = new Table().LeftAligned();
        table.Border(TableBorder.Rounded);
        AnsiConsole.Live(table)
        .Overflow(VerticalOverflow.Ellipsis)
        .Start(ctx =>
        {
            table.AddColumn("[bold]Tenant Name[/]");
            ctx.Refresh();

            table.AddColumn("[bold]Tenant Id[/]");
            ctx.Refresh();

            foreach (var tenant in tenants)
            {
                table.AddRow(new Markup($"[blue]{tenant.Data.DisplayName}[/]"), new Markup($"[green]{tenant.Data.TenantId}[/]"));
                ctx.Refresh();
                Thread.Sleep(250);
            }
        });
    }

    public static async Task<ResourceGroupResource> CreateResourceGroup(SubscriptionResource sub, string rgName)
    {
        ResourceGroupResource? resourceGroup = null;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots9)
           .SpinnerStyle(Style.Parse("blue"))
           .StartAsync("Creating Resource Group...", async ctx =>
           {
               AnsiConsole.MarkupLineInterpolated($"[green]Creating a resource group with name:[/] [blue]{rgName}[/]");
               var rgLro = await sub.GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.WestEurope));
               resourceGroup = rgLro.Value;
               AnsiConsole.MarkupLineInterpolated($"[green]Created a resource group with id:[/] [blue] {resourceGroup.Id}[/]");
           });
        return resourceGroup;
    }

    public static async Task DeleteResourceGroup(string rgName)
    {
        var setting = await Utils.LoadConfiguration();
        SubscriptionResource? sub = null;
        ArmClient client = null;

        if (setting.CustomTenant)
        {
            sub = await Authenticate(setting.CustomTenantId);
            var credential = new AzureCliCredential(new AzureCliCredentialOptions()
            {
                TenantId = setting.CustomTenantId
            });
            client = new ArmClient(credential);
        }
        else
        {
            sub = await Authenticate();
            client = new ArmClient(new DefaultAzureCredential());
        }

        await AnsiConsole.Status()
             .Spinner(Spinner.Known.Dots9)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Deleting Resource Group...", async ctx =>
            {
                AnsiConsole.MarkupLine($"[green]Fetching resourcegroup resource[/]");
                var rg = await sub.GetResourceGroupAsync(rgName);
                AnsiConsole.MarkupLineInterpolated($"[green]Deleting resource group:[/] [blue]{rgName}[/]");
                await client.GetResourceGroupResource(rg.Value.Id).DeleteAsync(Azure.WaitUntil.Completed);
                AnsiConsole.MarkupLine($"[green]Resource Group deleted[/]");
            });

    }
}
