# Minimal Azure test environment

This template creates a single-instance test environment in one resource group:

- Azure App Service on the Free F1 Linux tier
- Azure Database for PostgreSQL Flexible Server on `Standard_B1ms`
- 32 GB database storage, 7-day local backups, and no high availability
- HTTPS-only application hosting with Blazor Server affinity enabled

The App Service tier has no fixed hosting charge but has a daily compute quota and can sleep when idle. PostgreSQL is the primary cost and varies by region; review the Azure estimate before deployment. Stop the PostgreSQL server while the test environment is unused to reduce compute charges. Azure automatically restarts a stopped Flexible Server after seven days.

## Deploy

Install the Azure PowerShell modules and Bicep CLI, then run:

```powershell
.\infra\deploy-test.ps1 -Location uksouth
```

Authentication uses Microsoft Azure PowerShell. By default, the script generates the database password cryptographically in memory and does not print or persist it. An existing secure value can be supplied with `-DatabaseAdminPassword` when direct database administration is required.

The database permits connections from Azure services so the Free App Service can connect without paid private networking. This is suitable only for a temporary test environment. Use private endpoints and tighter networking for production.

The deployment creates infrastructure and application configuration. Publish the application directly to App Service with:

```powershell
.\infra\deploy-app.ps1 -WebAppName studymgt-test-app-g6ow5r6bjpgiy
```

This script publishes a compact Release build, uploads each runtime file through the authenticated App Service VFS API, and restarts the Web App. It avoids GitHub credentials, publish-profile files, and server-side Oryx builds. Publishing credentials remain in memory and are not printed or persisted.