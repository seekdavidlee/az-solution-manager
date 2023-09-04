param location string = resourceGroup().location
param appConfigName string = 'ts${uniqueString(resourceGroup().name)}'

resource config 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  location: location
  name: '${appConfigName}1'
  sku: {
    name: 'free'
  }
  properties: {
    disableLocalAuth: true
    enablePurgeProtection: false
    publicNetworkAccess: 'Enabled'
  }
}

resource config2 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  location: location
  name: '${appConfigName}2'
  sku: {
    name: 'free'
  }
  properties: {
    disableLocalAuth: true
    enablePurgeProtection: false
    publicNetworkAccess: 'Enabled'
  }
}
