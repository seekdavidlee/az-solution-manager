param storageName1 string = '${uniqueString(resourceGroup().name)}01'
param storageName2 string = '${uniqueString(resourceGroup().name)}02'
param location string = resourceGroup().location

resource str1 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName1
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
  }
  tags:{
    'x-used-by':'foo'
  }
}

resource str2 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName2
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
  }
  tags:{
    'x-used-by':'bar'
  }
}
