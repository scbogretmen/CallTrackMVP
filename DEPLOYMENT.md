# CallTrackMVP - Kurulum ve Çalıştırma

## Gereksinimler
- .NET 8 SDK
- Windows (LAN sunucusu için)

## Çözüm Yapısı

```
CallTrackMVP/
├── CallTrackMVP.sln
└── CallTrackMVP.Web/
    ├── Controllers/
    │   ├── AdminController.cs
    │   ├── AuthController.cs
    │   ├── CallLogsController.cs
    │   └── HomeController.cs
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── DbInitializer.cs
    ├── Migrations/
    ├── Models/
    │   ├── AppUser.cs
    │   ├── CallLog.cs
    │   └── ViewModels/
    ├── Services/
    │   ├── ExcelExportService.cs
    │   └── IExcelExportService.cs
    ├── Views/
    ├── Data/
    │   └── CallTrack.db          (otomatik oluşturulur)
    ├── appsettings.json
    └── Program.cs
```

## Adım Adım Kurulum

### 1. Veritabanı ve Migration
```powershell
cd d:\Projeler\Vestel\CallTrackMVP.Web
dotnet ef migrations add Initial
dotnet ef database update
```

### 2. Uygulamayı Çalıştırma
```powershell
dotnet run
```

Uygulama `https://0.0.0.0:50201` adresinde dinler (local + LAN erişimi).

### 3. Windows Firewall (LAN erişimi için)
Yönetici olarak PowerShell:
```powershell
New-NetFirewallRule -DisplayName "CallTrack MVP Port 50201" -Direction Inbound -LocalPort 50201 -Protocol TCP -Action Allow
```

### 4. Erişim
- **Sunucudan**: https://localhost:50201
- **Ağdan (LAN)**: https://&lt;SUNUCU_IP&gt;:50201

Sunucu IP adresini öğrenmek: `ipconfig` → IPv4 Adresi

## Sürekli Çalıştırma (Windows Service)

Uygulama bilgisayar açıldığında otomatik başlasın ve kullanıcı oturumu kapalı olsa bile çalışsın:

### Taşınabilir Kurulum (Sunucuya kopyalama sonrası)

1. **Geliştirme makinesinde** publish edin:
   ```powershell
   cd d:\Projeler\Vestel\CallTrackMVP.Web
   dotnet publish -c Release -o ./publish
   ```

2. **Publish klasörünü** sunucuya kopyalayın (örn: `C:\CallTrackMVP\`)

3. **Sunucuda**, publish klasörüne gidip **Yönetici** PowerShell'de:
   ```powershell
   cd C:\CallTrackMVP
   .\install-service.ps1
   ```

Script otomatik olarak:
- CallTrackMVP adlı Windows Service oluşturur
- Servisi başlatır
- Otomatik başlatma (StartupType: Automatic) ayarlar

**Not:** Windows Service olarak çalışırken HTTP kullanılır (sertifika kısıtı). Erişim: **http://localhost:50201** veya **http://&lt;SUNUCU_IP&gt;:50201**

### Yönetim
- **Servisi durdur**: `Stop-Service -Name CallTrackMVP`
- **Servisi başlat**: `Start-Service -Name CallTrackMVP`
- **Servis durumu**: `Get-Service -Name CallTrackMVP`
- **Servisi kaldır**: `.\uninstall-service.ps1` (Yönetici PowerShell)

### Güncelleme sonrası
Kod değişikliği yaptıktan sonra:
1. Yeniden publish edin ve publish klasörünü sunucuya kopyalayın
2. Sunucuda:
```powershell
cd C:\CallTrackMVP
.\uninstall-service.ps1
.\install-service.ps1
```

## Varsayılan Kullanıcılar

| Kullanıcı | Şifre      | Rol   |
|-----------|------------|-------|
| admin     | Admin123!  | Admin |
| ahmet     | Ahmet123!  | User  |
| ayse      | Ayse123!   | User  |
| kubra     | Kubra123!  | User  |
| mehmet    | Mehmet123! | User  |

## Sorun Giderme

### SQLite dosya yolu
- Veritabanı: `CallTrackMVP.Web/Data/CallTrack.db`
- `Data` klasörü yoksa uygulama ilk çalıştırmada otomatik oluşturur.
- Bağlantı: `Data Source=Data\CallTrack.db` (ContentRootPath'e göre)

### Veritabanı kilitli (SQLite locked)
- Sunucuda tek uygulama örneği çalıştığından emin olun.
- DbContext scoped kullanılıyor; her request için ayrı instance.
- Eşzamanlı yazma çok sık değilse SQLite WAL modu ile 4 kullanıcı sorunsuz çalışır.

### Port 50201 kullanımda
Program.cs'de UseUrls'i değiştirin veya ortam değişkeni:
```powershell
$env:ASPNETCORE_URLS="https://0.0.0.0:50202"; dotnet run
```
