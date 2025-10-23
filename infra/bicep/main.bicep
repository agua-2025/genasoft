param location string = 'brazilsouth'
param namePrefix string = 'genasoft'
param env string = 'dev'

module ai './ai.bicep' = {
  name: 'ai'
  params: {
    location: location
    namePrefix: namePrefix
    env: env
  }
}

module st './storage.bicep' = {
  name: 'st'
  params: {
    location: location
    namePrefix: namePrefix
    env: env
  }
}

module kv './keyvault.bicep' = {
  name: 'kv'
  params: {
    location: location
    namePrefix: namePrefix
    env: env
  }
}

module sql './sql.bicep' = {
  name: 'sql'
  params: {
    location: location
    namePrefix: namePrefix
    env: env
  }
}

module app './appservice.bicep' = {
  name: 'app'
  params: {
    location: location
    namePrefix: namePrefix
    env: env
    appInsightsId: ai.outputs.appInsightsId
    storageAccountName: st.outputs.accountName
  }
}

module swa './staticwebapp.bicep' = {
  name: 'swa'
  params: {
    location: location
    namePrefix: namePrefix
    env: env
  }
}

module sb './servicebus.bicep' = {
  name: 'sb'
  params: {
    location: location
    namePrefix: namePrefix
    env: env
  }
}

module fd './frontdoor.bicep' = {
  name: 'fd'
  params: {
    location: location
    namePrefix: namePrefix
    env: env
  }
}

output resourceGroupLocation string = location
