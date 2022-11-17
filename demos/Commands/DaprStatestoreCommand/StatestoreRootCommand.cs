using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using System.CommandLine;
using System.Xml.Linq;

namespace demos.Commands.DaprStatestoreCommand
{
    public class StatestoreRootCommand : Command
    {
        public StatestoreRootCommand() : base("state", "Dapr statestore demo")
        {
            var deployOption = new Option<bool>(
                name: "-deployOnly", description: "Create Azure environment to use in demo");
            deployOption.AddAlias("-d");
            AddOption(deployOption);

            var rgNameOption = new Option<string>(
                name: "-rg", "Name of the resource group if deploying");
            AddOption(rgNameOption);

            this.SetHandler(async (deploy, rgName) =>
            { await Demo(deploy, rgName); }, deployOption, rgNameOption);
        }

        private async Task Demo(bool deploy, string rgName)
        {
            try
            {
                //authenticate
                Console.WriteLine("Authenticating");
                var credential = new Azure.Identity.AzureCliCredential(new AzureCliCredentialOptions()
                {
                    TenantId = "16b3c013-d300-468d-ac64-7eda0820b6d3"
                });
                
                var client = new ArmClient(credential);
                var sub = await client.GetDefaultSubscriptionAsync();
                Console.WriteLine($"Default Subscription Name: {sub.Data.DisplayName}");

                if (deploy)
                    await CreateResources(client, sub, rgName);
                else
                { }
                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            //string path = AppDomain.CurrentDomain.BaseDirectory;
            //Console.WriteLine($"Path: {path}");

            //path = path +  @"Infra\Demo.ps1";
            //Console.WriteLine($"Path: {path}");
            ////create runspace
            //var runSpace = System.Management.Automation.Runspaces.RunspaceFactory.CreateRunspace();
            //runSpace.Open();

            //var pipe = runSpace.CreatePipeline();
            //var cmd = new System.Management.Automation.Runspaces.Command(path, true);
            //if(deploy)
            //{
            //    var para = new System.Management.Automation.Runspaces.CommandParameter("deployOnly");
            //    cmd.Parameters.Add(para);
            //}
            //pipe.Commands.Add(cmd);
            //var results = pipe.Invoke();
            //runSpace.Close();

            //foreach(var pso in results)
            //{
            //    Console.WriteLine(pso.ToString());
            //}
        }

        private async Task CreateResources(ArmClient client, SubscriptionResource sub, string rgName)
        {
            //Create resource group
            var rg = "dapr_state_demo";
            if (!string.IsNullOrEmpty(rgName))
                rg = rgName;
            Console.WriteLine($"Creating a resource group with name: {rg}");
            var rgLro = await sub.GetResourceGroups().CreateOrUpdateAsync(Azure.WaitUntil.Completed, rg, new ResourceGroupData(AzureLocation.WestEurope));
            var resourceGroup = rgLro.Value;
            Console.WriteLine($"Created a resource group with id: {resourceGroup.Id}");

        }
    }
}
