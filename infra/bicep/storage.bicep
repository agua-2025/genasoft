param location string
param namePrefix string
param env string

resource st 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: toLower('${namePrefix}st${env}')
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
  }
}

resource contExport 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${st.name}/default/export'
  properties: {
    publicAccess: 'None'
  }
}

resource contPreviews 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${st.name}/default/previews'
  properties: {
    publicAccess: 'None'
  }
}

output accountName string = st.name
