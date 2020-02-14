﻿using dotnetcore.oauth2Services.Models;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace dotnetcore.oauth2Services
{
    public class ClientCredentialsManager : IClientCredentialsManager
    {
        private IHttpClientFactory _httpClientFactory;
        private IDiscoveryCache _discoveryCache;
        private ClientCredentials _options;
        class TokenRecord
        {
            public TokenResponse TokenResponse { get; set; }
            public ClientCredential ClientCredential { get; set; }
            public DateTime Expiration { get; set; }
        }
        private Dictionary<string, TokenRecord> TokenRecords { get; set; }

        public ClientCredentialsManager(
            IHttpClientFactory httpClientFactory,
            IOptions<ClientCredentials> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            TokenRecords = new Dictionary<string, TokenRecord>();
        }

        IDiscoveryCache DiscoveryCache
        {
            get
            {
                if (_discoveryCache == null)
                {
                    _discoveryCache = new DiscoveryCache(_options.Authority, () => _httpClientFactory.CreateClient());
                }
                return _discoveryCache;
            }
        }

        public async Task<TokenResponse> GetAccessTokenAsync(string tenant)
        {
            var nowUtc = DateTime.UtcNow;
            TokenRecord tokenRecord = null;
            if (TokenRecords.TryGetValue(tenant, out tokenRecord))
            {
                var remaining = (tokenRecord.Expiration - nowUtc).TotalSeconds;
                if (remaining <= 0)
                {
                    tokenRecord = null;
                }
                else
                {
                    return tokenRecord.TokenResponse;
                }
            }

            return await RefreshAccessTokenAsync(tenant);
        }


        public async Task<TokenResponse> RefreshAccessTokenAsync(string tenant)
        {

            var clientCredentials = (from item in _options.ClientCredentialsClientCredentials
                                     where item.Tenant == tenant
                                     select item).FirstOrDefault();

            if (clientCredentials == null)
            {
                return null;
            }
            var tokenClient = _httpClientFactory.CreateClient("tokenClient");
            var document = await DiscoveryCache.GetAsync();

            var tokenResonse = await tokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
            {
                Address = document.TokenEndpoint,
                ClientId = clientCredentials.ClientId,
                ClientSecret = clientCredentials.ClientSecret,
            });
            TokenRecords[tenant] = new TokenRecord
            {
                TokenResponse = tokenResonse,
                ClientCredential = clientCredentials,
                Expiration = DateTime.UtcNow.AddSeconds(tokenResonse.ExpiresIn - 300)  // give a 5 minute skew.
            };

            return tokenResonse;
        }
    }
}
