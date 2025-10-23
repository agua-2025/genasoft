param location string
param namePrefix string
param env string
param appInsightsId string
param storageAccountName string

var appName = '${namePrefix}-api-${env}'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${namePrefix}-plan-${env}'
  location: location
  sku: {
    name: 'P1v3'
    tier: 'PremiumV3'
    capacity: 1
  }
}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  properties: {
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: reference(appInsightsId, '2020-02-02').ConnectionString
        }
        {
          name: 'Storage__BlobServiceUri'
          value: 'https://PLACEHOLDER.blob.core.windows.net/'
        }
      ]
    }
    serverFarmId: plan.id
  }
}
