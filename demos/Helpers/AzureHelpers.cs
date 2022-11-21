using Azure.Identity;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;

namespace demos.Helpers
{
    public static  class AzureHelpers
    {
        public  static async Task<SubscriptionResource> Authenticate()
        {
            //authenticate
            Console.WriteLine("Authenticating");
            var credential = new Azure.Identity.AzureCliCredential(new AzureCliCredentialOptions()
            {
                TenantId = "16b3c013-d300-468d-ac64-7eda0820b6d3"
            });

            var client = new ArmClient(credential);
            var sub = await client.GetDefaultSubscriptionAsync();
            Console.WriteLine("Authenticated");
            Console.WriteLine($"Default Subscription Name: {sub.Data.DisplayName}");
            return sub;
        }

        public static async Task<ResourceGroupResource> CreateResourceGroup(SubscriptionResource sub, string rgName)
        {
            Console.WriteLine($"Creating a resource group with name: {rgName}");
            var rgLro = await sub.GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.WestEurope));
            var resourceGroup = rgLro.Value;
            Console.WriteLine($"Created a resource group with id: {resourceGroup.Id}");
            return resourceGroup;
        }

        public static async Task DeleteResourceGroup(string rgName)
        {
            Console.WriteLine("Authenticating");
            var credential = new Azure.Identity.AzureCliCredential(new AzureCliCredentialOptions()
            {
                TenantId = "16b3c013-d300-468d-ac64-7eda0820b6d3"
            });

            var client = new ArmClient(credential);
            Console.WriteLine("Authenticated");
            Console.WriteLine("Fetching default subscription");
            var sub = await client.GetDefaultSubscriptionAsync();
            Console.WriteLine("Fetching resourcegroup resource");
            var rg = await sub.GetResourceGroupAsync(rgName);
            Console.WriteLine($"Deleting resource group with name: {rgName}");
            await client.GetResourceGroupResource(rg.Value.Id).DeleteAsync(Azure.WaitUntil.Completed);
            Console.WriteLine($"Resource Group deleted");
        }
    }
}
