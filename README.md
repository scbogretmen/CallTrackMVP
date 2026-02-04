# CallTrack MVP – Çağrı Takip

Müşteri çağrılarının kaydını tutan, takibini ve raporlamasını yapan web uygulaması. Yerel ağda (LAN) çalışır; Windows Service olarak sürekli çalışabilir.

## Özellikler

- Çağrı kayıtları oluşturma, düzenleme, listeleme
- Çağrı tiplerine göre filtreleme ve raporlama
- Kullanıcı yönetimi ve rol bazlı erişim (Admin / User)
- Excel’e dışa aktarma
- Windows Service veya tek kurulum EXE ile dağıtım

## Teknolojiler

- **.NET 8** · **ASP.NET Core MVC** · **SQLite** (Entity Framework Core)
- Cookie Authentication · ClosedXML (Excel) · Windows Service

## Hızlı Başlangıç

### Kurulum (kullanıcı tarafı)

1. **CallTrackMVP_Setup.exe** dosyasını indirip çalıştırın (yönetici izni verin).
2. Tarayıcıda **http://localhost:50201** açılır.
3. Varsayılan giriş: **admin** / **Admin123!** (ilk girişten sonra şifreyi değiştirin).

Kurulum EXE yoksa: Publish klasörünü kopyalayıp **Kurulum.bat** ile kurulum yapılabilir (ayrıntı için [KURULUM_KILAVUZU.md](KURULUM_KILAVUZU.md)).

### Geliştirme (geliştirici tarafı)

```bash
git clone https://github.com/scbogretmen/CallTrackMVP.git
cd CallTrackMVP
```

- **Visual Studio** veya **VS Code** ile `CallTrackMVP.sln` açın.
- `CallTrackMVP.Web` projesini çalıştırın; tarayıcıda https://localhost:50201 açılır.
- Veritabanı ilk çalıştırmada oluşturulur ve varsayılan kullanıcılar eklenir.

### Kurulum EXE üretme

Proje kökünde PowerShell:

```powershell
.\build-installer.ps1
```

Çıktı: `CallTrackMVP.Installer\bin\SetupOutput\CallTrackMVP_Setup.exe`

## Dokümantasyon

| Dosya | Açıklama |
|-------|----------|
| [KURULUM_KILAVUZU.md](KURULUM_KILAVUZU.md) | Kurulum, kullanım ve varsayılan ayarlar |
| [01_Proje_Ozet_ve_Mimari.md](01_Proje_Ozet_ve_Mimari.md) | Proje özeti ve mimari |
| [02_Kod_Referanslari.md](02_Kod_Referanslari.md) | Route, controller, model referansları |
| [03_Kritik_Kodlar.md](03_Kritik_Kodlar.md) | Güvenlik ve iş kuralları için kritik kodlar |
| [GUNCELLEMELER.md](GUNCELLEMELER.md) | Sürüm notları ve güncellemeler |

## Gereksinimler

- **Windows**
- Kurulum EXE kullanılıyorsa ek gereksinim yok; Publish + Kurulum.bat için **.NET 8 Runtime** gerekir.

## Lisans

Bu proje eğitim / iç kullanım amaçlıdır.
