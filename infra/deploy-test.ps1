[CmdletBinding()]
param(
    [string]$SubscriptionId,
    [string]$Location = "uksouth",
    [string]$ResourceGroupName = "rg-studymgt-test",
    [string]$NamePrefix = "studymgt-test",
    [SecureString]$DatabaseAdminPassword
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

if (-not $DatabaseAdminPassword) {
    $randomBytes = New-Object byte[] 48
    $randomNumberGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $randomNumberGenerator.GetBytes($randomBytes)
    }
    finally {
        $randomNumberGenerator.Dispose()
    }

    $generatedPassword = "Aa1!$([Convert]::ToBase64String($randomBytes).Replace('/', 'x').Replace('+', 'Y').Substring(0, 28))"
    $DatabaseAdminPassword = ConvertTo-SecureString $generatedPassword -AsPlainText -Force
    $generatedPassword = $null
}

if (-not (Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue)) {
    New-AzResourceGroup -Name $ResourceGroupName -Location $Location | Out-Null
}

$deployment = New-AzResourceGroupDeployment `
    -Name "studymgt-test-infrastructure" `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile "$PSScriptRoot\main.bicep" `
    -namePrefix $NamePrefix `
    -location $Location `
    -databaseAdminPassword $DatabaseAdminPassword

if ($deployment.ProvisioningState -ne "Succeeded") {
    throw "Azure deployment did not succeed. State: $($deployment.ProvisioningState)"
}

Write-Output "Web app: $($deployment.Outputs.webAppUrl.Value)"
Write-Output "Resource group: $ResourceGroupName"