Param(
    [ValidateSet('Development','QA','Production')]
    [string]$Environment = 'Development'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$hostConfig = Join-Path $root "src/ApiCf.Web.Host/appsettings.$Environment.json"

if (!(Test-Path $hostConfig)) {
    throw "Environment config not found: $hostConfig"
}

Write-Host "[ApiCf] Environment: $Environment"
Write-Host "[ApiCf] Config file: $hostConfig"
Write-Host "[ApiCf] Restoring dependencies..."

dotnet restore (Join-Path $root 'ApiCf.sln')

Write-Host "[ApiCf] Building solution..."

dotnet build (Join-Path $root 'ApiCf.sln') -c Release

Write-Host "[ApiCf] Setup completed successfully."
