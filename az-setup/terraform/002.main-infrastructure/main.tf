terraform {
  backend "azurerm" {
    # Due to a limitation in backend objects, variables cannot be passed in.
    # Do not declare an access_key here. Instead, export the
    # ARM_ACCESS_KEY environment variable.

    storage_account_name  = "stterraformshorturl"
    container_name        = "tstate"
    key                   = "terraform.tfstate"
  }
}
# Configure the Azure provider
provider "azurerm" {
  version = "=1.44.0"
}
resource "random_string" "suffix" {
  length  = 10
  upper   = false
  special = false
}
locals {
  resource_suffix              = random_string.suffix.result
}

resource "azurerm_resource_group" "rg" {
  name     = var.az_resource_group_name
  location = var.az_resource_group_location
}

resource "azurerm_application_insights" "main" {
  name                = "appis-shorturl"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
}

resource "azurerm_cosmosdb_account" "main" {
  name                = "cosmos-shorturl"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  enable_automatic_failover = false

  consistency_policy {
    consistency_level       = "Eventual"
    max_interval_in_seconds = 5
    max_staleness_prefix    = 100
  }

  geo_location {
    prefix            = "cosmos-shorturl-customid"
    location          = azurerm_resource_group.rg.location
    failover_priority = 0
  }
}
resource "azurerm_cosmosdb_sql_database" "shorturl_db" {
  name                = "shorturl"
  resource_group_name = azurerm_cosmosdb_account.main.resource_group_name
  account_name        = azurerm_cosmosdb_account.main.name
  throughput          = 400
}
resource "azurerm_cosmosdb_sql_container" "shorturl_container" {
  name                = "shorturl"
  resource_group_name = azurerm_cosmosdb_account.main.resource_group_name
  account_name        = azurerm_cosmosdb_account.main.name
  database_name       = azurerm_cosmosdb_sql_database.shorturl_db.name
  partition_key_path  = "/id"
  throughput          = 400
  default_ttl         = -1 
}
resource "azurerm_cosmosdb_sql_container" "expired_shorturl_container" {
  name                = "expired-shorturl"
  resource_group_name = azurerm_cosmosdb_account.main.resource_group_name
  account_name        = azurerm_cosmosdb_account.main.name
  database_name       = azurerm_cosmosdb_sql_database.shorturl_db.name
  partition_key_path  = "/id"
  throughput          = 400
  default_ttl         = -1 
}
 
resource "azurerm_storage_account" "azfunc_shorturl" {
  name                     = "stazfuncshorturl"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}
resource "azurerm_app_service_plan" "azfunc_consumption" {
  name                = "plan-shorturl"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}
resource "azurerm_function_app" "azfunc_shorturl" {
  name                      = "azfunc-shorturl"
  location                  = azurerm_resource_group.rg.location
  resource_group_name       = azurerm_resource_group.rg.name
  app_service_plan_id       = azurerm_app_service_plan.azfunc_consumption.id
  storage_connection_string = azurerm_storage_account.azfunc_shorturl.primary_connection_string
  identity { type = "SystemAssigned" }
  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                    = azurerm_application_insights.main.instrumentation_key,
    "APPLICATIONINSIGHTS_CONNECTION_STRING"             = format("InstrumentationKey=%s", azurerm_application_insights.main.instrumentation_key),
    "azFunc-shorturl-cosmos-primary-connection-string"  = var.azFunc_shorturl_cosmos_primary_connection_string,
    "azFunc-shorturl-cosmos-primarykey"                 = var.azFunc_shorturl_cosmos_primarykey,
    "azFunc-shorturl-cosmos-uri"                        = var.azFunc_shorturl_cosmos_uri,
    "jwt-validate-settings"                             = var.jwt_validate_settings,
    "FUNCTIONS_WORKER_RUNTIME"                          = "dotnet"
  }
  version="~3"

}



module "key_vault" {
  source = "innovationnorway/key-vault/azurerm"

  name = var.az_keyvault_name

  resource_group_name = azurerm_resource_group.rg.name

  access_policies = [
      {
        user_principal_names = ["hstahl@symantec.com"]
        secret_permissions   = ["backup","delete","get", "list","purge","recover","restore","set"]
      },
      {
        object_ids        = ["${azurerm_function_app.azfunc_shorturl.identity.0.principal_id}"]
        secret_permissions   = ["get"]
      },
      
  ]


  secrets = {
    "message" = "Hello, world!"
  }
}
