param name string
param location string

@allowed(['1','2','3'])
@minLength(1)
param zones array = [
  '1'
  '2'
  '3'
]

param loadBalancerPrivateIp string = ''
param loadBalancerSubnetId string

resource _lb 'Microsoft.Network/loadBalancers@2022-05-01' existing = {
  name: '${name}-lb'

  resource frontend 'frontendIPConfigurations' existing = {
    name: 'frontend'
  }

  resource backend 'backendAddressPools' existing = {
    name: 'backend'
  }
  resource probe 'probes' existing = {
    name: 'probe'
  }

  resource lbrule 'loadBalancingRules' existing = {
    name: 'lbrule'
  }
}

resource lb 'Microsoft.Network/loadBalancers@2021-05-01' = {
  name: _lb.name
  location: location
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    frontendIPConfigurations: [
      {
        name: _lb::frontend.name
        properties: {
          privateIPAddress: empty(loadBalancerPrivateIp) ? null : loadBalancerPrivateIp
          subnet: {
            id: loadBalancerSubnetId
          }
        }
      }
    ]
    backendAddressPools: [
      {
        name: _lb::backend.name
      }
    ]
    loadBalancingRules: [
      {
        name: _lb::lbrule.name
        properties: {
          loadDistribution: 'Default'
          frontendIPConfiguration: {
            id: _lb::frontend.id
          }
          backendAddressPool: {
            id: _lb::backend.id
          }
          protocol: 'All'
          frontendPort: 0
          backendPort: 0
          enableFloatingIP: false
          idleTimeoutInMinutes: 5
          probe: {
            id: _lb::probe.id
          }
        }
      }
    ]
    probes: [
      {
        name: _lb::probe.name
        properties: {
          protocol: 'tcp'
          port: 53
          intervalInSeconds: 5
          numberOfProbes: 1
        }
      }
    ]
  }

  resource probe 'probes' existing = {
    name: _lb::probe.name
  }
}

param vmssSkuName string = 'Standard_B1s'
@minValue(1)
param vmssCapacity int = length(zones)
@description('''
type vmssMaintananceWindowType = {
  startDateTime: string
  duration: string
  timeZone: string
  recurEvery: string
}
''')
param vmssMaintananceWindow /*vmssMaintananceWindowType*/ object = {
  startDateTime: '2023-01-21 01:00'
  duration: '05:00'
  timeZone: 'Central European Standard Time'
  recurEvery: '1Day'
}
@description('''
type vmssImageReferenceType = {
  publisher: string
  offer: string
  sku: string
  version: string
}
''')
param vmssImageReference /*vmssImageReferenceType*/ object = {
  publisher: 'Canonical'
  offer: '0001-com-ubuntu-server-jammy'
  sku: '22_04-lts'
  version: 'latest'
}
param vmssSubnetId string

param vmssAdminUsername string = 'vmroot'
@secure()
param vmssAdminSshKey string

resource vmss 'Microsoft.Compute/virtualMachineScaleSets@2022-08-01' = {
  name: '${name}-vmss'
  location: location
  sku: {
    name: vmssSkuName
    capacity: vmssCapacity
  }
  zones: zones
  properties: {
    overprovision: true
    singlePlacementGroup: true
    upgradePolicy: {
      mode: 'Rolling'
      automaticOSUpgradePolicy: {
        enableAutomaticOSUpgrade: true
        useRollingUpgradePolicy: true
      }
      rollingUpgradePolicy: {
        maxUnhealthyInstancePercent: 50
      }
    }
    virtualMachineProfile: {
      storageProfile: {
        osDisk: {
          createOption: 'FromImage'
          caching: 'ReadWrite'
          managedDisk: {
            storageAccountType: 'StandardSSD_LRS'
          }
        }
        imageReference: vmssImageReference
      }
      osProfile: {
        linuxConfiguration: {
          provisionVMAgent: true
          disablePasswordAuthentication: true
          ssh: {
            publicKeys: [
              {
                path: '/home/${vmssAdminUsername}/.ssh/authorized_keys'
                keyData: vmssAdminSshKey
              }
            ]
          }
        }
        computerNamePrefix: '${name}-'
        adminUsername: vmssAdminUsername
      }
      networkProfile: {
        healthProbe: {
          id: lb::probe.id
        }
        networkInterfaceConfigurations: [
          {
            name: 'nic'
            properties: {
              enableIPForwarding: true
              primary: true
              dnsSettings: {
                dnsServers: [
                  '168.63.129.16'
                ]
              }
              networkSecurityGroup: {
                id: nsg.id
              }
              ipConfigurations: [
                {
                  name: 'ipconfig'
                  properties: {
                    primary: true
                    subnet: {
                      id: vmssSubnetId
                    }
                    loadBalancerBackendAddressPools: [
                      {
                        id: _lb::backend.id
                      }
                    ]
                  }
                }
              ]
            }
          }
        ]
      }
      extensionProfile: {
        extensions: [
          {
            name: 'config-vm'
            properties: {
              publisher: 'Microsoft.Azure.Extensions'
              type: 'CustomScript'
              typeHandlerVersion: '2.1'
              autoUpgradeMinorVersion: false
              enableAutomaticUpgrade: false
              settings: {
                skipDos2Unix: true
              }
              protectedSettings: {
                script: loadFileAsBase64('init.sh')
              }
            }
          }
        ]
      }
      diagnosticsProfile: {
        bootDiagnostics: {
          enabled: false
        }
      }
    }
  }
}

resource maintananceConfiguration 'Microsoft.Maintenance/maintenanceConfigurations@2022-07-01-preview' = {
  name: '${vmss.name}-mtc'
  location: location
  tags: {
  }
  properties: {
    extensionProperties: {
    }
    maintenanceScope: 'OSImage'
    maintenanceWindow: vmssMaintananceWindow
    visibility: 'Custom'
  }
}

resource maintananceConfigurationDefaultAssignment 'Microsoft.Maintenance/configurationAssignments@2022-07-01-preview' = {
  name: 'defaultconfigurationassignment'
  location: location
  scope: vmss
  properties: {
    maintenanceConfigurationId: maintananceConfiguration.id
    resourceId: vmss.id
  }
} 
resource maintananceConfigurationAssignment 'Microsoft.Maintenance/configurationAssignments@2022-07-01-preview' = {
  name: '${maintananceConfiguration.name}assignment'
  location: location
  scope: vmss
  properties: {
    maintenanceConfigurationId: maintananceConfiguration.id
    resourceId: vmss.id
  }
}

resource nsg 'Microsoft.Network/networkSecurityGroups@2019-11-01' = {
  name: '${name}-nsg'
  location: location
  properties: {
  }
}
