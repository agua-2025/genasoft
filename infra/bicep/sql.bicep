param location string
param namePrefix string
param env string

resource sql 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: '${namePrefix}-sql-${env}'
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: 'PLACEHOLDER_PASSWORD'
  }
}

resource db 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  name: '${sql.name}/atosdb'
  properties: {
    sku: {
      name: 'GP_S_Gen5_2'
    }
  }
}
