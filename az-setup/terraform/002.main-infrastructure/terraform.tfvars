az_resource_group_location = "eastus2"
az_resource_group_name = "rg-shorturl"
az_keyvault_name = "kv-shorturl"
azFunc_shorturl_name = "azFunc-shorturl"

azFunc_shorturl_cosmos_primary_connection_string    = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/azFunc-shorturl-cosmos-primary-connection-string/90bdf88a107a43548d9d7c3c3ba30dd9)"
azFunc_shorturl_client_credentials                  = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/azFunc-shorturl-client-credentials/28c36b6fed8f443d8ea35647297a87b1)"
azFunc_shorturl_cosmos_primarykey                   = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/azFunc-shorturl-cosmos-primarykey/0956e636a74645998300e33b95f26e36)"
azFunc_shorturl_cosmos_uri                          = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/azFunc-shorturl-cosmos-uri/f9ba79e22c3c4e84ae68159bcc7966cc)"
jwt_validate_settings                               = "@Microsoft.KeyVault(SecretUri=https://kv-shorturl.vault.azure.net/secrets/jwt-validate-settings/621ecb8ae02c4a98912f5b7413798137)"


# export ARM_ACCESS_KEY=$(az keyvault secret show --name terraform-backend-key --vault-name kv-terraform-shorturl --query value -o tsv)  