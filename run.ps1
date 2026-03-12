# OneDrive / Antivirus "Access Denied" .exe engelini aşmak için DLL ile çalıştırır
# Kullanım: .\run.ps1  veya  powershell -ExecutionPolicy Bypass -File run.ps1

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot
$apiProject = Join-Path $projectDir "WhatShouldIWatch.API"
$outDir = Join-Path $apiProject "bin\Debug\net8.0"
$dllPath = Join-Path $outDir "WhatShouldIWatch.API.dll"

Write-Host "Building..." -ForegroundColor Cyan
$sln = Join-Path $projectDir "WhatShouldIWatch.API.sln"
dotnet build "$sln" -c Debug
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not (Test-Path $dllPath)) {
    Write-Host "DLL not found: $dllPath" -ForegroundColor Red
    exit 1
}

Write-Host "Starting API (DLL) on http://localhost:5000 ..." -ForegroundColor Green
Set-Location $outDir
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5000"
dotnet exec WhatShouldIWatch.API.dll
