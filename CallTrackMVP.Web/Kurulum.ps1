# CallTrack MVP - Tek tikla otomatik kurulum
# Yonetici yetkisi gerekir (Kurulum.bat cift tikla bunu saglar)

$ErrorActionPreference = "Stop"
$ServiceName = "CallTrackMVP"
$DisplayName = "CallTrack MVP - Cagri Takip"
$HedefKlasor = "C:\CallTrackMVP"
$VarsayilanPort = 50201
$YedekPort = 50202

# Yonetici kontrolu - degilse kendini yonetici olarak yeniden baslat
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "Yonetici yetkisi isteniyor..." -ForegroundColor Yellow
    $scriptPath = Join-Path $PSScriptRoot "Kurulum.ps1"
    Start-Process powershell -ArgumentList "-ExecutionPolicy Bypass -File `"$scriptPath`"" -Verb RunAs -Wait
    exit
}

# Script'in bulundugu klasor (kullanici nereye cikarttıysa)
$KaynakKlasor = $PSScriptRoot
$ExeAdi = "CallTrackMVP.Web.exe"
$ExeYolu = Join-Path $KaynakKlasor $ExeAdi

if (-not (Test-Path $ExeYolu)) {
    Write-Host "HATA: $ExeAdi bulunamadi. Tum Publish dosyalarini bir klasore cikartip Kurulum.bat'i o klasorde calistirin." -ForegroundColor Red
    Read-Host "Cikmak icin Enter'a basin"
    exit 1
}

Write-Host "CallTrack MVP kurulumu basliyor..." -ForegroundColor Cyan

# C:\CallTrackMVP olustur ve tum calisacak dosyalari oraya kopyala (proje nerede olursa olsun)
$KurulumKlasoru = $HedefKlasor
if (-not (Test-Path $KurulumKlasoru)) {
    New-Item -ItemType Directory -Path $KurulumKlasoru -Force | Out-Null
    Write-Host "$KurulumKlasoru olusturuldu." -ForegroundColor Cyan
}
Write-Host "Calisacak dosyalar $KurulumKlasoru konumuna kopyalaniyor..." -ForegroundColor Cyan
robocopy.exe "$KaynakKlasor" "$KurulumKlasoru" /E /IS /IT /NFL /NDL /NJH /NJS /NC /NS /NP | Out-Null
if ($LASTEXITCODE -ge 8) {
    Write-Host "Kopyalama hatasi. Manuel olarak tum dosyalari $KurulumKlasoru icerisine kopyalayip tekrar deneyin." -ForegroundColor Red
    Read-Host "Cikmak icin Enter'a basin"
    exit 1
}
Write-Host "Kopyalama tamamlandi." -ForegroundColor Green

$ExePath = Join-Path $KurulumKlasoru $ExeAdi
if (-not (Test-Path $ExePath)) {
    Write-Host "HATA: Kopyalama sonrasi $ExeAdi bulunamadi." -ForegroundColor Red
    Read-Host "Cikmak icin Enter'a basin"
    exit 1
}

# Port secimi: 50201 mesgulse 50202 kullan
$KullanilacakPort = $VarsayilanPort
$PortMesgul = Get-NetTCPConnection -LocalPort $VarsayilanPort -State Listen -ErrorAction SilentlyContinue
if ($PortMesgul) {
    Write-Host "Port $VarsayilanPort kullanımda, $YedekPort kullanilacak." -ForegroundColor Yellow
    $KullanilacakPort = $YedekPort
    [Environment]::SetEnvironmentVariable("ASPNETCORE_URLS", "http://0.0.0.0:$YedekPort", "Machine")
} else {
    # Varsayilan port kullanilacaksa eski ortam degiskenini temizle (onceki kurulumda 50202 secilmis olabilir)
    [Environment]::SetEnvironmentVariable("ASPNETCORE_URLS", $null, "Machine")
}

# Guvenlik duvari kurali (varsa atla)
$KuralAdi = "CallTrack MVP Port $KullanilacakPort"
$mevcutKural = Get-NetFirewallRule -DisplayName $KuralAdi -ErrorAction SilentlyContinue
if (-not $mevcutKural) {
    Write-Host "Guvenlik duvari kurali ekleniyor (ag erisimi icin)..." -ForegroundColor Cyan
    New-NetFirewallRule -DisplayName $KuralAdi -Direction Inbound -LocalPort $KullanilacakPort -Protocol TCP -Action Allow -ErrorAction SilentlyContinue
}

# Mevcut servisi kaldir
$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Eski servis kaldiriliyor..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

# Servisi olustur ve baslat
Write-Host "Windows servisi kuruluyor..." -ForegroundColor Cyan
try {
    New-Service -Name $ServiceName -DisplayName $DisplayName -BinaryPathName "`"$ExePath`"" -StartupType Automatic | Out-Null
    Start-Service -Name $ServiceName
    Write-Host "Servis baslatildi." -ForegroundColor Green
} catch {
    Write-Host "Servis kurulum hatasi: $_" -ForegroundColor Red
    Read-Host "Cikmak icin Enter'a basin"
    exit 1
}

# Bir kac saniye bekleyip tarayici ac
Start-Sleep -Seconds 2
$Url = "http://localhost:$KullanilacakPort"
Write-Host ""
Write-Host "Kurulum tamamlandi." -ForegroundColor Green
Write-Host "Tarayicida aciliyor: $Url" -ForegroundColor Cyan
Write-Host "Bu bilgisayardan: $Url" -ForegroundColor White
Write-Host "Agdaki diger bilgisayarlardan: http://<bu-bilgisayarin-ip-adresi>:$KullanilacakPort" -ForegroundColor White
Write-Host ""
Start-Process $Url

Read-Host "Kapatmak icin Enter'a basin"
