param aksName string

var defaultPool = {
  name: 'system'
  properties: {
    count: 1
    osType: 'Linux'
    mode: 'System'
    vmSize: 'Standard_B2ms'
  }
}
resource aksCluster 'Microsoft.ContainerService/managedClusters@2021-10-01' = {
  name: aksName
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    kubernetesVersion: '1.22.4'
    dnsPrefix: 'dnsprefix'
    enableRBAC: true
    nodeResourceGroup: '${aksName}-cluster-rg'
    agentPoolProfiles:[
      union({
        name: defaultPool.name
      }, defaultPool.properties)
    ]
  }
  resource system 'agentPools' = {
    name: defaultPool.name
    properties: defaultPool.properties
  }

  resource userpool1 'agentPools' = {
    name: 'userpool1'
    properties: {
      count: 1
      osType: 'Linux'
      mode: 'User'
      vmSize: 'Standard_B2s'
      availabilityZones: [
        '1'
      ]
    }
  }
  
  resource userpool2 'agentPools' = {
    name: 'userpool2'
    properties: {
      count: 1
      osType: 'Linux'
      mode: 'User'
      vmSize: 'Standard_B2s'
      availabilityZones: [
        '2'
      ]
    }
  }
}
