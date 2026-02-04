# CallTrack MVP - Tek kurulum EXE olusturma
# Cozumun kok dizininde calistirin: .\build-installer.ps1

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
$WebProj = Join-Path $Root "CallTrackMVP.Web\CallTrackMVP.Web.csproj"
$InstallerProj = Join-Path $Root "CallTrackMVP.Installer\CallTrackMVP.Installer.csproj"
$PayloadDir = Join-Path $Root "CallTrackMVP.Installer\_payload"
$PayloadZip = Join-Path $Root "CallTrackMVP.Installer\payload.zip"
$SetupOut = Join-Path $Root "CallTrackMVP.Installer\bin\SetupOutput"

Write-Host "CallTrack MVP - Kurulum EXE olusturuluyor..." -ForegroundColor Cyan

# 1) Web uygulamasini publish et
Write-Host "1/4 Web uygulamasi publish ediliyor..." -ForegroundColor Yellow
if (Test-Path $PayloadDir) { Remove-Item $PayloadDir -Recurse -Force }
dotnet publish $WebProj -c Release -o $PayloadDir
if ($LASTEXITCODE -ne 0) { Write-Host "Publish hatasi." -ForegroundColor Red; exit 1 }

# 2) Kurulum scriptlerini payload'a kopyala (Web publish ciktisina zaten dahil olabilir; Publish klasorundekileri de kopyalayalim)
$KurulumPs1 = Join-Path $Root "Publish\Kurulum.ps1"
$KurulumBat = Join-Path $Root "Publish\Kurulum.bat"
foreach ($f in @($KurulumPs1, $KurulumBat)) {
    if (Test-Path $f) { Copy-Item $f -Destination $PayloadDir -Force }
}
# Proje icindeki Kurulum dosyalari da olabilir
$WebKurulumPs1 = Join-Path $Root "CallTrackMVP.Web\Kurulum.ps1"
$WebKurulumBat = Join-Path $Root "CallTrackMVP.Web\Kurulum.bat"
foreach ($f in @($WebKurulumPs1, $WebKurulumBat)) {
    if (Test-Path $f) { Copy-Item $f -Destination $PayloadDir -Force }
}

# 3) payload.zip olustur (Installer proje klasorune)
Write-Host "2/4 payload.zip olusturuluyor..." -ForegroundColor Yellow
if (Test-Path $PayloadZip) { Remove-Item $PayloadZip -Force }
Compress-Archive -Path (Join-Path $PayloadDir "*") -DestinationPath $PayloadZip -CompressionLevel Optimal
Write-Host "   payload.zip: $([math]::Round((Get-Item $PayloadZip).Length / 1MB, 2)) MB" -ForegroundColor Gray

# 4) Installer'i tek EXE olarak publish et (gomulu payload ile)
Write-Host "3/4 Kurulum EXE derleniyor..." -ForegroundColor Yellow
if (Test-Path $SetupOut) { Remove-Item $SetupOut -Recurse -Force }
dotnet publish $InstallerProj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $SetupOut
if ($LASTEXITCODE -ne 0) { Write-Host "Installer publish hatasi." -ForegroundColor Red; exit 1 }

# Temizlik: gecici payload klasoru (opsiyonel)
if (Test-Path $PayloadDir) { Remove-Item $PayloadDir -Recurse -Force }

$SetupExe = Join-Path $SetupOut "CallTrackMVP_Setup.exe"
Write-Host ""
Write-Host "4/4 Tamamlandi." -ForegroundColor Green
Write-Host "Kurulum EXE: $SetupExe" -ForegroundColor Green
Write-Host "Boyut: $([math]::Round((Get-Item $SetupExe).Length / 1MB, 2)) MB" -ForegroundColor Gray
Write-Host ""
Write-Host "Bu dosyayi dagitabilirsiniz; kullanici cift tiklayip yonetici izni vererek kurulum yapar." -ForegroundColor Cyan
