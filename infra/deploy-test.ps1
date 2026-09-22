[CmdletBinding()]
param(
    [string]$SubscriptionId,
    [string]$Location = "uksouth",
    [string]$ResourceGroupName = "rg-studymgt-test",
    [string]$NamePrefix = "studymgt-test"
)

$ErrorActionPreference = "Stop"

Import-Module Az.Accounts
Import-Module Az.Resources

$context = Get-AzContext
if (-not $context) {
    Connect-AzAccount -UseDeviceAuthentication | Out-Null
    $context = Get-AzContext
}

if ($SubscriptionId) {
    Set-AzContext -SubscriptionId $SubscriptionId | Out-Null
}

$databasePassword = Read-Host "Enter a strong PostgreSQL administrator password" -AsSecureString

New-AzResourceGroup -Name $ResourceGroupName -Location $Location -Force | Out-Null

$deployment = New-AzResourceGroupDeployment `
    -Name "studymgt-test-infrastructure" `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile "$PSScriptRoot\main.bicep" `
    -namePrefix $NamePrefix `
    -location $Location `
    -databaseAdminPassword $databasePassword

if ($deployment.ProvisioningState -ne "Succeeded") {
    throw "Azure deployment did not succeed. State: $($deployment.ProvisioningState)"
}

Write-Output "Web app: $($deployment.Outputs.webAppUrl.Value)"
Write-Output "Resource group: $ResourceGroupName"