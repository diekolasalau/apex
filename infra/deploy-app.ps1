[CmdletBinding()]
param(
    [string]$ResourceGroupName = "rg-studymgt-test",
    [Parameter(Mandatory)]
    [string]$WebAppName
)

$ErrorActionPreference = "Stop"
$publishDirectory = Join-Path $env:TEMP "StudyMgt-publish-$([guid]::NewGuid().ToString('N'))"
$authorization = $null
$password = $null
$username = $null

try {
    dotnet publish "$PSScriptRoot\..\StudyMgt.csproj" `
        --configuration Release `
        --no-restore `
        --output $publishDirectory

    if ($LASTEXITCODE -ne 0) {
        throw "Application publish failed."
    }

    $publishedFiles = @(Get-ChildItem $publishDirectory -File -Recurse)
    $publishedBytes = [int64](($publishedFiles | Measure-Object Length -Sum).Sum)

    if (-not (Test-Path (Join-Path $publishDirectory "StudyMgt.dll") -PathType Leaf)) {
        throw "StudyMgt.dll was not produced."
    }

    if ($publishedBytes -ge 250MB) {
        throw "Publish output exceeds the 250 MB test-environment limit."
    }

    [xml]$profiles = Get-AzWebAppPublishingProfile `
        -ResourceGroupName $ResourceGroupName `
        -Name $WebAppName
    $profile = $profiles.publishData.publishProfile |
        Where-Object { $_.userPWD } |
        Select-Object -First 1

    if (-not $profile) {
        throw "App Service publishing credentials are unavailable."
    }

    $username = [string]$profile.userName
    $password = [string]$profile.userPWD
    $authorization = "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${username}:$password")))"
    $headers = @{ Authorization = $authorization; "If-Match" = "*" }
    $vfsRoot = "https://$WebAppName.scm.azurewebsites.net/api/vfs/site/wwwroot"
    $uploadedFiles = 0

    foreach ($file in $publishedFiles) {
        $relativePath = $file.FullName.Substring($publishDirectory.Length).TrimStart('\').Replace('\', '/')
        $encodedPath = (($relativePath -split '/') | ForEach-Object { [Uri]::EscapeDataString($_) }) -join '/'

        Invoke-WebRequest `
            -Uri "$vfsRoot/$encodedPath" `
            -Method Put `
            -Headers $headers `
            -ContentType "application/octet-stream" `
            -InFile $file.FullName `
            -UseBasicParsing | Out-Null

        $uploadedFiles++
    }

    Restart-AzWebApp -ResourceGroupName $ResourceGroupName -Name $WebAppName | Out-Null

    Write-Output "Uploaded $uploadedFiles files ($publishedBytes bytes) to $WebAppName."
    Write-Output "Application URL: https://$WebAppName.azurewebsites.net"
}
finally {
    $authorization = $null
    $password = $null
    $username = $null

    if (Test-Path $publishDirectory) {
        Remove-Item $publishDirectory -Recurse -Force
    }
}