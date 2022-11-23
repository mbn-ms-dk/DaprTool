using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace demos.Models
{
    public class SecretJsonPubsub
    {
        [JsonPropertyName("ipAddress")]
        public string IPAddress { get; set; }
        [JsonPropertyName("databaseName")]
        public string DatabaseName { get; set; }
        [JsonPropertyName("sqlConnectionString")]
        public string SqlConnectionString { get; set; }
        [JsonPropertyName("eventHubsEndpoint")]
        public string EventHubsEndpoint { get; set; }
        [JsonPropertyName("serviceBusEndpoint")]
        public string ServiceBusEndpoint { get; set; }
        [JsonPropertyName("storageAccountName")]
        public string StorageAccountName { get; set; }
        [JsonPropertyName("storageAccountKey")]
        public string StorageAccountKey { get; set; }
    }
}
