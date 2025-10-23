param location string
param namePrefix string
param env string

resource swa 'Microsoft.Web/staticSites@2022-03-01' = {
  name: '${namePrefix}-web-${env}'
  location: location
  properties: {
    repositoryUrl: 'https://github.com/ORG/REPO' // placeholder
    branch: 'main'
    buildProperties: {
      appLocation: 'web'
      outputLocation: '.next'
    }
  }
}
