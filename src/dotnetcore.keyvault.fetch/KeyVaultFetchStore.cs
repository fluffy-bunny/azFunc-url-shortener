using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnetcore.keyvault.fetch
{
    public class KeyVaultFetchStore<T>
        where T : class
    {
        private KeyVaultClientStoreOptions<T> _options;
        private ILogger<KeyVaultFetchStore<T>> _logger;
        private T _value;
        private DateTime _lastRead;

        public KeyVaultFetchStore(
            IOptions<KeyVaultClientStoreOptions<T>> options,
            ILogger<KeyVaultFetchStore<T>> logger)
        {
            _options = options.Value;
            _logger = logger;
        }
        protected virtual void AugmentFetchedValue(ref T value)
        {

        }
        async Task<T> HardFetchAsync()
        {
            try
            {
                if (_options.Value != null)
                {
                    return _options.Value;
                }
                /* The next four lines of code show you how to use AppAuthentication library to fetch secrets from your key vault */
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync(_options.KeyVaultSecretUrl);
                var value = JsonConvert.DeserializeObject<T>(secret.Value);
                AugmentFetchedValue(ref value);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
        async Task FetchAsync()
        {
            if (_value == null)
            {
                _value = await HardFetchAsync();
                _lastRead = DateTime.UtcNow;
            }
            else
            {
                System.TimeSpan diff = DateTime.UtcNow.Subtract(_lastRead);
                if (diff.TotalSeconds > _options.ExpirationSeconds)
                {
                    _value = await HardFetchAsync();
                    _lastRead = DateTime.UtcNow;
                }
            }
        }
        public async Task<T> GetValueAsync()
        {
            await FetchAsync();
            return _value;
        }
    }
}
