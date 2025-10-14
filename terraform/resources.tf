# Locals
locals {
  resource_group_name = var.resource_group_name != "" ? var.resource_group_name : "rg-${var.app_name}-${var.environment}"
  function_app_name   = "func-${var.app_name}-${var.environment}"
  storage_account_name = "st${var.app_name}${var.environment}"
  app_service_plan_name = "asp-${var.app_name}-${var.environment}"
  app_insights_name   = "appi-${var.app_name}-${var.environment}"
  
  common_tags = merge(
    var.tags,
    {
      Environment = var.environment
      ManagedBy   = "Terraform"
      Application = var.app_name
    }
  )
}

# Resource Group
resource "azurerm_resource_group" "rg" {
  name     = local.resource_group_name
  location = var.location
  tags     = local.common_tags
}

# Storage Account for Function App
resource "azurerm_storage_account" "storage" {
  name                     = local.storage_account_name
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = local.common_tags
}

# Application Insights
resource "azurerm_application_insights" "app_insights" {
  name                = local.app_insights_name
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  application_type    = "web"
  tags                = local.common_tags
}

# App Service Plan (Linux)
resource "azurerm_service_plan" "app_service_plan" {
  name                = local.app_service_plan_name
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "Y1" # Consumption plan
  tags                = local.common_tags
}

# Linux Function App
resource "azurerm_linux_function_app" "function_app" {
  name                       = local.function_app_name
  resource_group_name        = azurerm_resource_group.rg.name
  location                   = azurerm_resource_group.rg.location
  service_plan_id            = azurerm_service_plan.app_service_plan.id
  storage_account_name       = azurerm_storage_account.storage.name
  storage_account_access_key = azurerm_storage_account.storage.primary_access_key
  
  site_config {
    application_stack {
      dotnet_version              = "9.0"
      use_dotnet_isolated_runtime = true
    }
    
    application_insights_connection_string = azurerm_application_insights.app_insights.connection_string
    application_insights_key               = azurerm_application_insights.app_insights.instrumentation_key
  }
  
  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"       = "dotnet-isolated"
    "WEBSITE_RUN_FROM_PACKAGE"       = "1"
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.app_insights.instrumentation_key
  }
  
  tags = local.common_tags
}
