@description('Short lowercase prefix used for globally unique resource names.')
@minLength(3)
@maxLength(20)
param namePrefix string = 'studymgt-test'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('PostgreSQL administrator username.')
param databaseAdminUsername string = 'studymgtadmin'

@secure()
@description('PostgreSQL administrator password.')
param databaseAdminPassword string

var uniqueSuffix = uniqueString(resourceGroup().id)
var appServicePlanName = '${namePrefix}-plan'
var webAppName = take('${namePrefix}-app-${uniqueSuffix}', 60)
var databaseServerName = take('${namePrefix}-pg-${uniqueSuffix}', 63)
var databaseName = 'studymgt'

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: 'F1'
    tier: 'Free'
    size: 'F1'
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

resource databaseServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: databaseServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: databaseAdminUsername
    administratorLoginPassword: databaseAdminPassword
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    storage: {
      storageSizeGB: 32
      autoGrow: 'Disabled'
    }
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: databaseServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource applicationDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: databaseServer
  name: databaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'DemoData__SeedTutorTimesheetSample'
          value: 'false'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Host=${databaseServer.properties.fullyQualifiedDomainName};Port=5432;Database=${databaseName};Username=${databaseAdminUsername};Password=${databaseAdminPassword};SSL Mode=Require;Trust Server Certificate=true'
        }
      ]
    }
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output databaseServerName string = databaseServer.name
output estimatedFixedResources array = [
  'App Service F1 (free, subject to daily compute quota)'
  'PostgreSQL Flexible Server Standard_B1ms with 32 GB storage'
]