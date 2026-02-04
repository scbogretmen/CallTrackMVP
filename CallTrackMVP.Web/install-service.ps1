# CallTrackMVP - Windows Service Kurulum (Publish klasöründen çalıştırın)
# YÖNETİCİ olarak PowerShell'de çalıştırın
# Kullanım: Publish klasörüne gidip .\install-service.ps1

$ServiceName = "CallTrackMVP"
$DisplayName = "CallTrack MVP - Cagri Takip"
$ScriptDir = $PSScriptRoot
$ExePath = Join-Path $ScriptDir "CallTrackMVP.Web.exe"

if (-not (Test-Path $ExePath)) {
    Write-Host "HATA: CallTrackMVP.Web.exe bulunamadi. Bu scripti publish klasoru icinde calistirin." -ForegroundColor Red
    Write-Host "Ornek: dotnet publish -c Release -o ./publish  sonra  cd publish  ve  .\install-service.ps1" -ForegroundColor Yellow
    exit 1
}

# Mevcut servisi kaldır
$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Mevcut servis kaldiriliyor..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
}

# Servisi oluştur (çalışma dizini olarak script klasörü kullanılacak)
Write-Host "Windows Service kuruluyor..." -ForegroundColor Cyan
try {
    New-Service -Name $ServiceName -DisplayName $DisplayName -BinaryPathName "`"$ExePath`"" -StartupType Automatic
    Write-Host "Servis kuruldu!" -ForegroundColor Green
    Write-Host "Servis baslatiliyor..." -ForegroundColor Cyan
    Start-Service -Name $ServiceName -ErrorAction Stop
    Write-Host "Servis calisiyor. Erisim: http://localhost:50201" -ForegroundColor Green
} catch {
    Write-Host "HATA: $_" -ForegroundColor Red
    Write-Host "Olay Gorusu'nden (Event Viewer) CallTrackMVP hatalarini kontrol edin." -ForegroundColor Yellow
}
