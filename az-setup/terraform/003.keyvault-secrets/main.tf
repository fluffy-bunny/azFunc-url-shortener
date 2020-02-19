terraform {
  backend "azurerm" {
    # Due to a limitation in backend objects, variables cannot be passed in.
    # Do not declare an access_key here. Instead, export the
    # ARM_ACCESS_KEY environment variable.

    storage_account_name  = "stterraformshorturl"
    container_name        = "tstate"
    key                   = "terraform.tfstate.keyvault.secrets"
  }
}
# Configure the Azure provider
provider "azurerm" {
  version = "=1.44.0"
}

data "azurerm_resource_group" "rg" {
  name     = var.az_resource_group_name
}
data "azurerm_key_vault" "main" {
  name = var.az_keyvault_name
  resource_group_name         = data.azurerm_resource_group.rg.name
}
resource "azurerm_key_vault_secret" "main" {
  for_each     = {
    "message" = "Hello, world!",
    "azFunc-shorturl-client-credentials" = "eyJhdXRob3JpdHkiOiJodHRwczovL2FwaW0tb3JnYW5pY3MuYXp1cmUtYXBpLm5ldC9vYXV0aDIiLCJ0ZW5hbnRzIjpbeyJjcmVkZW50aWFscyI6eyJjbGllbnRfaWQiOiJiMmItY2xpZW50IiwiY2xpZW50X3NlY3JldCI6InNlY3JldCJ9LCJuYW1lIjoibWFya2V0aW5nIiwib3B0aW9ucyI6eyJtYXhfdHRsIjoyNTkyMDAwfX1dfQ=="
  }
  name         = each.key
  value        = each.value
  key_vault_id = data.azurerm_key_vault.main.id
}