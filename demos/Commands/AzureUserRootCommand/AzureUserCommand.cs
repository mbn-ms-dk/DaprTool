using demos.Models;
using System.CommandLine;

namespace demos.Commands.AzureUserRootCommand
{
    public class AzureUserCommand : Command
    {
        public AzureUserCommand() : base("user", "Different options to authenticate against Azure")
        {
            var defSubOption = new Option<bool>(name: "--default", description: "Default Azure Subscription for your account");
            AddOption(defSubOption);

            var listTenantOption = new Option<bool>(name: "--list-tenant", "List tenants available for you");
            AddOption(listTenantOption);

            var listSubsOption = new Option<bool>(name: "--list-sub", description: "List subscriptions available in your current tenant");
            AddOption(listSubsOption);


            var switchTenantOption = new Option<string>(name: "--tenant", description: "Id of tenant to switch to");
            AddOption(switchTenantOption);

            this.SetHandler(async (def, tenant, subs, tenantId) =>
            {
                await AzureOperations(def, tenant, subs, tenantId);
            }, defSubOption, listTenantOption, listSubsOption, switchTenantOption);
        }

        private async Task AzureOperations(bool def, bool tenant, bool subs, string tenantId)
        {
            if (def)
            {
                await Helpers.AzureHelpers.GetDefaultSubscription();
            }
            else if (tenant)
            {
                await Helpers.AzureHelpers.ListTenants();
            }
            else if (subs)
            {
                await Helpers.AzureHelpers.ListSubscriptions();
            }
            else if (!string.IsNullOrWhiteSpace(tenantId))
            {
                var sub = await Helpers.AzureHelpers.Authenticate(tenantId);
                var settings = await Helpers.Utils.LoadConfiguration();
                settings.CustomTenant = true;
                settings.CustomTenantId = tenantId;
                await Helpers.Utils.SaveConfiguration(settings);
            }
        }
    }
}
