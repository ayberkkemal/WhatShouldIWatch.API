# Build oncesi DLL kilidini acmak icin calisan dotnet surecini durdurur, sonra build alir.
# Kullanim: .\build.ps1

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot
$sln = Join-Path $projectDir "WhatShouldIWatch.API.sln"

$dotnets = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnets) {
    Write-Host "DLL kilitli - $($dotnets.Count) dotnet sureci durduruluyor (PID: $($dotnets.Id -join ', '))..." -ForegroundColor Yellow
    $dotnets | Stop-Process -Force
    Start-Sleep -Seconds 2
}

Write-Host "Building..." -ForegroundColor Cyan
dotnet build "$sln" -c Debug
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Build basarili." -ForegroundColor Green
