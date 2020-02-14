using Newtonsoft.Json;

namespace dotnetcore.oauth2Services.Models
{
    public partial class ClientCredential
    {
        [JsonProperty("tenant")]
        public string Tenant { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }
}
