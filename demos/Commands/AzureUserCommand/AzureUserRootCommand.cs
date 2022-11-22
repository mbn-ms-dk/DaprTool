using demos.Helpers;
using demos.Models;
using System.CommandLine;

namespace demos.Commands.AzureUserCommand
{
    public  class AzureUserRootCommand : Command
    {
        public AzureUserRootCommand() : base("user", "Different options to authenticate against Azure")
        {
            var defSubOption = new Option<bool>(
                name: "--def", description: "List Default Azure Subscription for your account");
            AddOption(defSubOption);

            var listOption = new Option<bool>(
                name: "--list", "List tenants available for you");
            AddOption(listOption);

            var switchTenantOption = new Option<string>(
                name: "--tenant", description: "Id of tenant to switch to");
            AddOption(switchTenantOption);

            this.SetHandler(async (def, list, swi) =>
            { await AzureOperations(def, list, swi); }, defSubOption, listOption, switchTenantOption);
        }

        private async Task AzureOperations(bool def, bool list, string swi)
        {

            if (def)
            {
                await AzureHelpers.GetDefaultSubscription();
            }
            else if(list)
            {
                AzureHelpers.ListTenants();
            }
            else if(!string.IsNullOrWhiteSpace(swi))
            {
                var sub = await AzureHelpers.Authenticate(swi);
                await Helpers.Utils.SaveConfiguration(new Settings()
                {
                    CustomTenant = true,
                    CustomTenantId = swi
                });
            }
        }
    }
}
