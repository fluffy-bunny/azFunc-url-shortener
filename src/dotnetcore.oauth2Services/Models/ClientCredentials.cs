using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace dotnetcore.oauth2Services.Models
{
    public partial class ClientCredentials
    {
        [JsonProperty("authority")]
        public string Authority { get; set; }
        [JsonProperty("client_credentials")]
        public List<ClientCredential> ClientCredentialsClientCredentials { get; set; }
    }
}
