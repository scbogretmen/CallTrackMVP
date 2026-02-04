# CallTrackMVP - Windows Service Kaldırma
# YÖNETİCİ olarak PowerShell'de çalıştırın

$ServiceName = "CallTrackMVP"

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $service) {
    Write-Host "Servis bulunamadi: $ServiceName" -ForegroundColor Yellow
    exit 0
}

Write-Host "Servis durduruluyor..." -ForegroundColor Cyan
Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue

Write-Host "Servis kaldiriliyor..." -ForegroundColor Cyan
sc.exe delete $ServiceName

Write-Host "Servis kaldirildi." -ForegroundColor Green
