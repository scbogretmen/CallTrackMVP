# 01 – Proje Özet ve Mimari

## Proje Özeti

**CallTrack MVP** (Çağrı Takip), müşteri çağrılarının kaydını tutan, takibini ve raporlamasını yapan bir web uygulamasıdır. Yerel ağda (LAN) çalışır; Windows Service olarak sürekli çalışabilir.

### Amaç

- Çağrı kayıtlarını oluşturma, güncelleme ve listeleme  
- Çağrı tiplerine göre filtreleme ve raporlama  
- Kullanıcı yönetimi ve rol bazlı erişim (Admin / User)  
- Excel’e dışa aktarma  

### Hedef Kullanım

- Küçük ve orta ölçekli ekipler  
- Yerel ağda tek sunucu üzerinde çalışma  
- Kurulumu kolay, tek paket veya Publish klasörü ile dağıtım  

---

## Teknoloji Yığını

| Katman      | Teknoloji |
|-------------|-----------|
| **Framework** | .NET 8 |
| **Web**       | ASP.NET Core MVC |
| **Veritabanı**| SQLite (Entity Framework Core) |
| **Kimlik Doğrulama** | Cookie Authentication (ASP.NET Core Identity benzeri) |
| **Excel**     | ClosedXML |
| **Hosting**   | Kestrel, Windows Service desteği |

---

## Çözüm Yapısı

```
Vestel/
├── CallTrackMVP.sln
├── CallTrackMVP.Web/           # Ana web uygulaması
│   ├── Controllers/
│   ├── Data/
│   ├── Migrations/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   ├── wwwroot/
│   └── Program.cs
├── CallTrackMVP.Installer/     # Tek paket kurulum EXE üreticisi
│   └── Program.cs
├── Publish/                    # Dağıtım klasörü (exe, dll, scriptler)
├── build-installer.ps1         # Kurulum EXE oluşturma scripti
├── KURULUM_KILAVUZU.md
├── GUNCELLEMELER.md
└── 01_Proje_Ozet_ve_Mimari.md
```

---

## Uygulama Mimarisi

### MVC Yapısı

- **Controllers:** Home, Auth, CallLogs, Admin  
- **Views:** Razor (.cshtml), Shared layout  
- **Models:** AppUser, CallLog, CallType, CallLogUpdate, CallLogAcknowledgment  
- **Services:** ExcelExportService (ClosedXML ile Excel export)  

### Veri Modeli (Özet)

| Tablo | Açıklama |
|-------|----------|
| **AppUsers** | Kullanıcılar (UserName, PasswordHash, Role: Admin/User) |
| **CallLogs** | Çağrı kayıtları (Tarih, ÇağrıNo, Müşteri, Açıklama, vb.) |
| **CallTypes** | Çağrı tipleri (Web, Telefon, Eposta vb.) |
| **CallLogUpdates** | Çağrı güncelleme geçmişi |
| **CallLogAcknowledgments** | Kullanıcı bazlı okundu bilgisi |

### Kimlik Doğrulama

- Cookie tabanlı oturum (8 saat, kayan oturum)  
- Rol: Admin (tam yetki), User (kısıtlı yetki)  
- Giriş: `/Auth/Login`, Çıkış: `/Auth/Logout`  

### Veritabanı

- **SQLite** (`Data\CallTrack.db`)  
- EF Core Migrations ile şema yönetimi  
- DbInitializer ile varsayılan kullanıcı ve çağrı tipleri  

---

## Dağıtım Mimarisi

### Çalışma Modları

| Mod | Protokol | Port | Kullanım |
|-----|----------|------|----------|
| **Windows Service** | HTTP | 50201 | Sunucuda sürekli çalışma, otomatik başlatma |
| **Geliştirme** | HTTPS | 50201 | `dotnet run` ile local geliştirme |

### Kurulum Seçenekleri

1. **Tek paket EXE:** CallTrackMVP_Setup.exe – tüm bileşenler içinde, .NET gerektirmez  
2. **Publish + Kurulum.bat:** Publish klasörü + Kurulum.bat ile C:\CallTrackMVP’e kurulum  
3. **Manuel:** Publish’i C:\CallTrackMVP’e kopyalayıp `install-service.ps1` çalıştırma  

### Ağ Erişimi

- **Sunucudan:** http://localhost:50201  
- **LAN’dan:** http://sunucu-ip;:50201  
- Windows Güvenlik Duvarı kuralı kurulum script’i ile eklenir  

---

## Proje Bağımlılıkları

- ClosedXML – Excel oluşturma  
- Microsoft.EntityFrameworkCore.Sqlite – SQLite ORM  
- Microsoft.Extensions.Hosting.WindowsServices – Windows Service desteği  
- Microsoft.Extensions.Identity.Core – Şifre hash (PasswordHasher)  

---

## İlgili Dokümanlar

- **KURULUM_KILAVUZU.md** – Kurulum ve kullanım adımları  
- **GUNCELLEMELER.md** – Sürüm notları ve güncellemeler  
