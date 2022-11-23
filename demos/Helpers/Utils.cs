using demos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace demos.Helpers
{
    public enum DaprType
    {
        Binding,
        State,
        Pubsub
    }
    public static class Utils
    {
        private static readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "/components/appl/settings.json";
        private static readonly string bindingJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/components/binding/azure/local_secrets.json";
        private static readonly string stateJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/components/state/azure/local_secrets.json";
        private static readonly string pubsubJsonPath = AppDomain.CurrentDomain.BaseDirectory + "/components/pubsub/azure/local_secrets.json";
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
            var url = "http://checkip.dyndns.org";
            var client = new HttpClient();
            var response = await client.GetStringAsync(url);
            var ipAddressWithText = response.Split(':');
            var ipAddressWithHTMLEnd = ipAddressWithText[1].Substring(1);
            var ipAddress = ipAddressWithHTMLEnd.Split('<');
            var mainIP = ipAddress[0];
            return mainIP;
        }

        public static async Task<Settings?> LoadConfiguration()
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
            return JsonSerializer.Deserialize<Settings>(setting);
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
            using var fs = File.Create(path);
            fs.Close();
            await File.AppendAllTextAsync(path, json);
        }
    }
}
