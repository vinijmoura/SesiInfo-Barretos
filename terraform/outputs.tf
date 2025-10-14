output "function_app_name" {
  description = "Name of the Function App"
  value       = azurerm_linux_function_app.function_app.name
}

output "function_app_default_hostname" {
  description = "Default hostname of the Function App"
  value       = azurerm_linux_function_app.function_app.default_hostname
}

output "function_app_id" {
  description = "ID of the Function App"
  value       = azurerm_linux_function_app.function_app.id
}

output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.rg.name
}
