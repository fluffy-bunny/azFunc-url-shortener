az_resource_group_location = "eastus2"
az_resource_group_name = "rg-shorturl"
az_keyvault_name = "kv-shorturl"
azFunc_shorturl_name = "azFunc-shorturl"

azFunc_shorturl_cosmos_primary_connection_string = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/azFunc-shorturl-cosmos-primary-connection-string/8be2558efbbf4dafa81a2658a0b0f087)"
azFunc_shorturl_client_credentials = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/azFunc-shorturl-client-credentials/df3bd1e663a0452694d2543d1dc5e453)"
azFunc_shorturl_cosmos_primarykey = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/azFunc-shorturl-cosmos-primarykey/29040c61e22f422a907fd05020e93c6e)"
azFunc_shorturl_cosmos_uri = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/azFunc-shorturl-cosmos-uri/9c9c11f38a84403ea7be6c83f92820f0)"


# export ARM_ACCESS_KEY=$(az keyvault secret show --name terraform-backend-key --vault-name kv-terraform-shorturl --query value -o tsv)  