resource "azurerm_resource_group" "rg" {
  name     = "todomanager-rg"
  location = "West Europe"
}

resource "azurerm_app_service_plan" "plan" {
  name                = "todomanager-plan"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "Linux"
  reserved            = true

  sku {
    tier = "Basic"
    size = "B1"
  }
}

resource "azurerm_linux_web_app" "app" {
  name                = "todomanager-app"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id     = azurerm_app_service_plan.plan.id

  site_config {
    application_stack {
      docker_image     = "nginx"
      docker_image_tag = "latest"
    }
  }

  app_settings = {
    WEBSITES_PORT = "5172"
  }
}
