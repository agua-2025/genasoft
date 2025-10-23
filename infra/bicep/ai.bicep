param location string
param namePrefix string
param env string

resource ai 'Microsoft.Insights/components@2020-02-02' = {
  name: '${namePrefix}-ai-${env}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

output appInsightsId string = ai.id
