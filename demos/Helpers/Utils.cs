using demos.Models;
using Spectre.Console;
using System.Text.Json;

namespace demos.Helpers
{
    public enum DaprType
    {
        Binding,
        State,
        Pubsub,
        Secrets
    }
    public static class Utils
    {
        private static readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "/components/appl/settings.json";
        private static readonly string bindingJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/components/binding/azure/local_secrets.json";
        private static readonly string stateJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/components/state/azure/local_secrets.json";
        private static readonly string pubsubJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/components/pubsub/azure/local_secrets.json";
        private static readonly string secretsJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/components/secrets/azure/local_secrets.json";
        public static string GenerateRandomString(int length)
        {
            var res = new Random();

            var str = "abcdefghijklmnopqrstuvwxyz0123456789";

            var randomstring = string.Empty;

            for (int i = 0; i < length; i++)
            {
                int x = res.Next(str.Length);
                randomstring = randomstring + str[x];
            }
            return randomstring;
        }

        public static async Task<string> GetPublicIpAddress()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("https://api.ipify.org?format=json");//("https://api.myip.com/");
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<MyIp>(resp);
            return json.MyIpAddress;
        }

        public static async Task<Settings> LoadConfiguration()
        {
            if (!File.Exists(configPath))
            {
                var json = JsonSerializer.Serialize(new Settings()
                {
                    CustomTenant = false,
                    CustomTenantId = string.Empty
                },
                new JsonSerializerOptions() { WriteIndented = true });
                await File.AppendAllTextAsync(configPath, json);
            }
            var setting = await File.ReadAllTextAsync(configPath);
            var result = JsonSerializer.Deserialize<Settings>(setting);
            return result == null ? new Settings() : result;
        }

        public static async Task SaveConfiguration(Settings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true });
            using var sw = File.Create(configPath);
            sw.Close();
            await File.AppendAllTextAsync(configPath, json);
        }

        public static async Task SaveSecretsFile(DaprType dapr, string json)
        {
            var path = string.Empty;
            if (dapr == DaprType.Binding)
                path = bindingJsonPath;
            else if (dapr == DaprType.State)
                path = stateJsonPath;
            else if (dapr == DaprType.Pubsub)
                path = pubsubJsonPath;
            else if (dapr == DaprType.Secrets)
                path = secretsJsonPath;
            using var fs = File.Create(path);
            fs.Close();
            await File.AppendAllTextAsync(path, json);
        }

        public static async Task<string> LoadSecretsFile(DaprType dapr)
        {
            var path = string.Empty;
            if (dapr == DaprType.Binding)
                path = bindingJsonPath;
            else if (dapr == DaprType.State)
                path = stateJsonPath;
            else if (dapr == DaprType.Pubsub)
                path = pubsubJsonPath;
            else if (dapr == DaprType.Secrets)
                path = secretsJsonPath;

            return await File.ReadAllTextAsync(path);
        }

        public static void ShowDemoDescription(DaprType daprType)
        {
            var txt = string.Empty;

            if (daprType == DaprType.Binding)
                txt = "The component in the components/binding/azure folder is configured to use Azure Blob Storage." + Environment.NewLine +
                      "and the local component in the components/binding/local is configured to use local file storage." + Environment.NewLine +
                      "The point to make comparing the files is that as long as the name of the component (in this demo 'files') does not change, " +
                      "the code will work no matter what backing service is used.";
            if (daprType == DaprType.State)
                txt = "The components/state sub folder contains components for local and Azure." + Environment.NewLine +
                       "These folders allow you to show the difference between the default components (local) and the components in the other folders." + Environment.NewLine + 
                       "The point to make comparing the files is that as long as the name of the component does not change the code will work no matter what backing service is used.";
            if (daprType == DaprType.Pubsub)
                txt = @"This folder holds the components/pubsub/azure, components, deploy, and pubsub project folders." + Environment.NewLine +
                    "The components/pubsub/azure and components folders are so you can show the difference between a local component" + Environment.NewLine +
                    "and a component configured for the cloud. The component in the components/pubsub/azure folder is configured to use Azure Service Bus." + Environment.NewLine +
                    "and the local component is configured to use Redis." + Environment.NewLine + 
                    "The point to make comparing the files is that as long as the name of the component does not change the code will work no matter what backing service is used.";
            if (daprType == DaprType.Secrets)
                txt = @"The components/azure and components folders are in the workspace so you can show the difference between a local component and a component configured for the cloud." + Environment.NewLine +
                    "The component in the components/azure folder is configured to use Azure Key Vault and the local component is configured to use local file." + Environment.NewLine + 
                    "The point to make comparing the files is that as long as the name of the component does not change the code will work no matter what backing service is used.";

            var panel = new Panel(txt)
            {
                Header = new PanelHeader($"Dapr {daprType.ToString().ToLower()} demo"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 2, 2, 2),
                BorderStyle = new Style(foreground: Color.Blue)
            };
            AnsiConsole.Write(panel);
        }
    }
}
