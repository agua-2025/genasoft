param location string
param namePrefix string
param env string

resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${namePrefix}-kv-${env}'
  location: location
  properties: {
    tenantId: '00000000-0000-0000-0000-000000000000' // placeholder
    sku: {
      name: 'standard'
      family: 'A'
    }
    enableSoftDelete: true
    enableRbacAuthorization: true
  }
}
