resource aksCluster 'Microsoft.ContainerService/managedClusters@2021-03-01' = {
  name: 'miq-aks'
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    kubernetesVersion: '1.22.0'
    dnsPrefix: 'dnsprefix'
    enableRBAC: true
    agentPoolProfiles: [
      {
        name: 'systempool'
        count: 1
        vmSize: 'Standard_B2ms'
        osDiskSizeGB: 128
        osType: 'Linux'
        mode: 'System'
        enableNodePublicIP: false
        enableAutoScaling: false
        nodeTaints: [
          'CriticalAddonsOnly=true:NoSchedule'
        ]
      }
      {
        name: 'workpool'
        count: 1
        vmSize: 'Standard_B2ms'
        osDiskSizeGB: 128
        osType: 'Linux'
        mode: 'User'
        enableNodePublicIP: false
        enableAutoScaling: false
        nodeTaints: []
      }
    ]
  }
}
