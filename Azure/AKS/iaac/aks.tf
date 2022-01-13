resource "azurerm_resource_group" "k8s" {
  name     = var.resource_group_name
  location = var.location
}

resource "random_id" "log_analytics_workspace_name_suffix" {
  byte_length = 8
}

resource "azurerm_log_analytics_workspace" "logs" {
  # The WorkSpace name has to be unique across the whole of azure, not just the current subscription/tenant.
  name                = "${var.log_analytics_workspace_name}-${random_id.log_analytics_workspace_name_suffix.dec}"
  location            = var.log_analytics_workspace_location
  resource_group_name = azurerm_resource_group.k8s.name
  sku                 = "PerGB2018"
}

resource "azurerm_log_analytics_solution" "logs" {
  solution_name         = "ContainerInsights"
  location              = azurerm_log_analytics_workspace.logs.location
  resource_group_name   = azurerm_resource_group.k8s.name
  workspace_resource_id = azurerm_log_analytics_workspace.logs.id
  workspace_name        = azurerm_log_analytics_workspace.logs.name

  plan {
    publisher = "Microsoft"
    product   = "OMSGallery/ContainerInsights"
  }
}

resource "azurerm_kubernetes_cluster" "k8s" {
  name                = var.cluster_name
  location            = azurerm_resource_group.k8s.location
  resource_group_name = azurerm_resource_group.k8s.name
  dns_prefix          = var.dns_prefix
  node_resource_group = var.aks_nodes_resource_group_name
  kubernetes_version  = "1.22.4"
  identity {
    type                      = "UserAssigned"
    user_assigned_identity_id = azurerm_user_assigned_identity.aks_identity.id
  }

  linux_profile {
    admin_username = "ubuntu"

    ssh_key {
      key_data = file(var.ssh_public_key)
    }
  }
  default_node_pool {
    name                         = "systempool"
    node_count                   = 1
    vm_size                      = "Standard_B2ms"
    enable_auto_scaling          = false
    only_critical_addons_enabled = true
    os_disk_size_gb              = 128
    type                         = "VirtualMachineScaleSets"
    os_disk_type                 = "Managed"
    vnet_subnet_id               = azurerm_subnet.aks_system.id
  }

  addon_profile {
    oms_agent {
      enabled                    = true
      log_analytics_workspace_id = azurerm_log_analytics_workspace.logs.id
    }
  }
  network_profile {
    load_balancer_sku  = "Standard"
    network_plugin     = "kubenet"
    docker_bridge_cidr = "172.17.0.1/16"
    service_cidr       = "10.254.0.0/16"
    dns_service_ip     = "10.254.0.10"
    pod_cidr           = "10.255.0.0/16"
  }

  tags = {
    Environment = "Development"
  }
}

resource "azurerm_user_assigned_identity" "aks_identity" {
  name                = "aks-id"
  resource_group_name = azurerm_resource_group.k8s.name
  location            = var.location
}

resource "azurerm_role_assignment" "aks_subet_system_contributor" {
  scope                = azurerm_subnet.aks_system.id
  role_definition_name = "Network Contributor"
  principal_id         = azurerm_user_assigned_identity.aks_identity.principal_id
}

resource "azurerm_role_assignment" "aks_subet_worker_contributor" {
  scope                = azurerm_subnet.aks_worker.id
  role_definition_name = "Network Contributor"
  principal_id         = azurerm_user_assigned_identity.aks_identity.principal_id
}

resource "azurerm_kubernetes_cluster_node_pool" "workerpool" {
  kubernetes_cluster_id = azurerm_kubernetes_cluster.k8s.id
  name                  = "workerpool"
  min_count             = 0
  max_count             = 2
  vm_size               = "Standard_B2ms"
  enable_auto_scaling   = true
  os_disk_size_gb       = 128
  os_disk_type          = "Managed"
  mode                  = "User"
  os_type               = "Linux"
  enable_node_public_ip = false
  vnet_subnet_id        = azurerm_subnet.aks_worker.id
}

resource "azurerm_virtual_network" "vnet" {
  name                = "aks-vnet"
  location            = azurerm_resource_group.k8s.location
  resource_group_name = azurerm_resource_group.k8s.name
  address_space       = ["10.0.100.0/24"]
}

resource "azurerm_subnet" "core" {
  name                 = "core"
  resource_group_name  = azurerm_resource_group.k8s.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.0.100.0/27"]
}
resource "azurerm_subnet" "aks_system" {
  name                 = "aks-system"
  resource_group_name  = azurerm_resource_group.k8s.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.0.100.32/27"]
}
resource "azurerm_subnet" "aks_worker" {
  name                 = "aks-worker"
  resource_group_name  = azurerm_resource_group.k8s.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.0.100.64/27"]
}
