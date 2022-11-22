using demos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace demos.Helpers
{
    public  static class Utils
    {
        private static  readonly string config = AppDomain.CurrentDomain.BaseDirectory + "/components/appl/settings.json";
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

        public static async Task<Settings?> LoadConfiguration()
        {
            if (!File.Exists(config))
            {
                var json = JsonSerializer.Serialize(new Settings()
                {
                    CustomTenant = false,
                    CustomTenantId = string.Empty
                }, new JsonSerializerOptions() {  WriteIndented = true});
                await File.AppendAllTextAsync(config, json);
            }
            var setting = await File.ReadAllTextAsync(config);
            return JsonSerializer.Deserialize<Settings>(setting);
        }

        public static async Task SaveConfiguration(Settings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true });
            File.Create(config);
            await File.AppendAllTextAsync(config, json);
        }
    }
}
