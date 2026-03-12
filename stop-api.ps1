# Build sırasında "file is being used by another process" hatası alıyorsan
# bu script'i çalıştırıp ardından tekrar build al.
# Çalışan .NET Host (dotnet exec / API) süreçlerini durdurur.

$dotnets = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if (-not $dotnets) {
    Write-Host "Calisan dotnet sureci yok. Dogrudan build alabilirsiniz." -ForegroundColor Green
    exit 0
}

Write-Host "Durduruluyor: $($dotnets.Count) dotnet sureci (PID: $($dotnets.Id -join ', '))" -ForegroundColor Yellow
$dotnets | Stop-Process -Force
Write-Host "Tamam. Simdi 'dotnet build' calistirabilirsiniz." -ForegroundColor Green
