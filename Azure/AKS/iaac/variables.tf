variable "agent_count" {
    default = 1
}

variable "ssh_public_key" {
    default = "~/.ssh/id_rsa.pub"
}

variable "dns_prefix" {
    default = "akscluster"
}

variable cluster_name {
    default = "aks-cluster"
}

variable resource_group_name {
    default = "aks-rg"
}
variable aks_nodes_resource_group_name {
    default = "aks-cluster-rg"
}


variable location {
    default = "westeurope"
}

variable log_analytics_workspace_name {
    default = "aks-logs"
}

# refer https://azure.microsoft.com/global-infrastructure/services/?products=monitor for log analytics available regions
variable log_analytics_workspace_location {
    default = "westeurope"
}

variable subscription_id {
    default = null
}

variable acr_name {
  
}
variable admin_group_object_id {
  
}
