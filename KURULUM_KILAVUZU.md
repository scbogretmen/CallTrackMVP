# CallTrack MVP – Kurulum Kılavuzu

Bu kılavuz, CallTrack MVP (Çağrı Takip) uygulamasının kurulumu, çalıştırılması ve program aktifken geçerli olan varsayılan ayarları açıklar. Sürüm notları ve güncellemeler için **GUNCELLEMELER.md** dosyasına bakın.

---

## 1. Gereksinimler

| Gereksinim | Açıklama |
|------------|----------|
| **İşletim Sistemi** | Windows |
| **.NET** | Ağdaki kurulum bilgisayarında **.NET 8 Runtime** yeterlidir (SDK veya proje kaynak kodu gerekmez). Publish klasörünü oluşturan makinede .NET 8 SDK gerekir. |
| **Yetki** | Windows Service kurulumu için Yönetici (Administrator) |

---

## 2. Kurulum ve Kullanım (Ağdaki Bir Bilgisayara Kurulum)

### 2.1 Tek Paket – Kurulum EXE (En Kolay, Önerilen)

**CallTrackMVP_Setup.exe** tek bir dosyadır; içinde uygulama ve kurulum vardır. Kullanıcıya sadece bu dosyayı verirsiniz.

**Yapılacaklar:**

1. **CallTrackMVP_Setup.exe** dosyasını hedef bilgisayara kopyalayın (USB, ağ, e‑posta vb.).
2. Dosyaya **çift tıklayın**.
3. Windows “Bu uygulama bilgisayarınızda değişiklik yapmak istiyor” diyorsa **Evet** deyin (yönetici izni).
4. Kurulum otomatik ilerler: **C:\CallTrackMVP** oluşturulur, dosyalar çıkarılır, Windows servisi kurulur, tarayıcı açılır.
5. Açılan sayfada **http://localhost:50201** (veya 50202) ile giriş yapın. Varsayılan kullanıcılar bölüm 3.4’te.

**Not:** Kurulum EXE tek paket olduğu için **.NET 8 Runtime** ayrıca yüklü olmasa da çalışır (paket kendi çalışma ortamını içerir). Solution veya Publish klasörü gerekmez.

**Kurulum EXE’yi nasıl üretirsiniz?**  
Proje kök dizininde (solution’ın olduğu klasörde) PowerShell ile:
```powershell
.\build-installer.ps1
```
Çıktı: `CallTrackMVP.Installer\bin\SetupOutput\CallTrackMVP_Setup.exe`

### 2.2 Publish Klasörü + Kurulum.bat

Publish klasörünün tamamına sahip olan kullanıcılar için:

1. **Publish** klasörünün **tüm içeriğini** hedef bilgisayara kopyalayın. Klasör adı/konumu önemli değil.
2. **Kurulum.bat** dosyasına **çift tıklayın**, yönetici izni verin.
3. Kurulum otomatik ilerler (C:\CallTrackMVP, servis, tarayıcı). Giriş: **http://localhost:50201** (veya 50202).

**Not:** Bu yöntemde bilgisayarda **.NET 8 Runtime** yüklü olmalıdır.

### 2.3 Manuel Kurulum (İsteğe Bağlı)

PowerShell ile kendiniz kurmak isterseniz:

**Adım 1 –** Publish içeriğini hedef bilgisayarda **C:\CallTrackMVP** klasörüne kopyalayın.

**Adım 2 –** **Yönetici** olarak PowerShell açın ve:

```powershell
cd C:\CallTrackMVP
.\install-service.ps1
```

**Adım 3 –** Tarayıcıda **http://localhost:50201** yazın (port değiştirdiyseniz o portu kullanın).

### 2.4 Publish Klasörünü veya Kurulum EXE’yi Kim Oluşturur?

Publish klasörü, projeyi geliştiren veya derleyen kişi tarafından **bir kez** oluşturulur. Ağdaki diğer bilgisayarlarda sadece bu hazır klasör kullanılır; o bilgisayarlarda solution açılması veya Visual Studio gerekmez.

**Publish oluşturmak (sadece geliştirici/build makinesinde):**

1. Çözümü açın: `CallTrackMVP.sln`
2. Proje klasörüne gidip publish alın:
   ```powershell
   cd d:\Projeler\Vestel\CallTrackMVP.Web
   dotnet publish -c Release -o ..\Publish
   ```
3. **Tek paket dağıtmak için:** Proje kökünde `.\build-installer.ps1` çalıştırıp **CallTrackMVP_Setup.exe** üretin; bu dosyayı dağıtın (bkz. **2.1**).  
   **Publish klasörü dağıtmak için:** Oluşan **Publish** içeriğini kopyalayıp **Kurulum.bat**’e çift tıklamalarını söyleyin (bkz. **2.2**).

---

## 3. Program Aktifken Varsayılan Ayarlar

Uygulama çalışırken aşağıdaki ayarlar varsayılan olarak geçerlidir (özel yapılandırma yapılmadığı sürece).

### 3.1 Ağ ve Erişim

| Ayar | Windows Service (Sunucu) | Geliştirme (dotnet run) |
|------|---------------------------|--------------------------|
| **Protokol** | HTTP | HTTPS |
| **Port** | 50201 | 50201 (HTTPS), 50202 (HTTP – launchSettings) |
| **Dinlenen adres** | `http://0.0.0.0:50201` | `https://0.0.0.0:50201` |
| **Sunucudan erişim** | http://localhost:50201 | https://localhost:50201 |
| **LAN’dan erişim** | http://SUNUCU_IP:50201 | https://BILGISAYAR_IP:50201 |

**Not:** Service modunda tarayıcıda mutlaka **http://** yazın; yoksa “Bu site güvenli bağlantı sağlayamıyor” hatası alabilirsiniz.

### 3.2 Veritabanı

| Ayar | Varsayılan Değer |
|------|-------------------|
| **Tür** | SQLite |
| **Bağlantı dizesi** | `Data Source=Data\CallTrack.db` |
| **Dosya konumu** | Uygulama çalışma dizini altında `Data\CallTrack.db` |
| **Data klasörü** | Yoksa ilk çalıştırmada otomatik oluşturulur |

### 3.3 Kimlik Doğrulama ve Oturum

| Ayar | Varsayılan Değer |
|------|-------------------|
| **Şema** | Cookie Authentication |
| **Giriş sayfası** | /Auth/Login |
| **Çıkış** | /Auth/Logout |
| **Oturum süresi** | 8 saat |
| **Kayan oturum** | Açık (aktivite ile süre uzar) |

### 3.4 Varsayılan Kullanıcılar (İlk kurulumda oluşturulur)

| Kullanıcı adı | Şifre | Rol | Tam ad |
|---------------|--------|-----|--------|
| admin | Admin123! | Admin | Yönetici |
| ahmet | Ahmet123! | User | Ahmet |
| ayse | Ayse123! | User | Ayşe |
| kubra | Kubra123! | User | Kübra |
| mehmet | Mehmet123! | User | Mehmet |

**Güvenlik:** İlk girişten sonra şifreleri mutlaka değiştirin.

### 3.5 Varsayılan Çağrı Tipleri

İlk kurulumda veritabanına eklenen çağrı tipleri:

- Web  
- Telefon  
- Eposta  

### 3.6 Uygulama Yapılandırması (appsettings.json)

| Ayar | Varsayılan |
|------|------------|
| **AllowedHosts** | * (tüm hostlara izin) |
| **Logging – Default** | Information |
| **Logging – Microsoft.AspNetCore** | Warning |

### 3.7 Diğer Varsayılanlar

- **Hata sayfası (Production):** /Home/Error  
- **HSTS:** Production’da açık (Service modunda yalnızca HTTP kullanıldığı için HTTPS yönlendirmesi yapılmaz)

---

## 4. Servis Yönetimi

| İşlem | Komut (Yönetici PowerShell) |
|-------|-----------------------------|
| Servisi başlat | `Start-Service -Name CallTrackMVP` |
| Servisi durdur | `Stop-Service -Name CallTrackMVP` |
| Durumu kontrol et | `Get-Service -Name CallTrackMVP` |
| Servisi kaldır | `cd C:\CallTrackMVP` ardından `.\uninstall-service.ps1` |

Windows’ta **Hizmetler** (services.msc) üzerinden “CallTrack MVP - Çağrı Takip” hizmetini de yönetebilirsiniz.

---

## 5. LAN Erişimi (Ağdan Kullanım)

Sunucuya ağ üzerinden erişmek için:

1. **Sunucu IP adresini** öğrenin: `ipconfig` → IPv4 Adresi  
2. **Windows Güvenlik Duvarı** kuralı ekleyin (Yönetici PowerShell):
   ```powershell
   New-NetFirewallRule -DisplayName "CallTrack MVP Port 50201" -Direction Inbound -LocalPort 50201 -Protocol TCP -Action Allow
   ```
3. İstemci tarayıcıda: **http://SUNUCU_IP:50201**

---

## 6. Güncelleme Sonrası

Kod değişikliği veya yeni sürüm sonrası:

1. Yeniden publish edin; Publish klasörünü sunucuya kopyalayın (mevcut klasörün üzerine yazın).  
2. Sunucuda:
   ```powershell
   cd C:\CallTrackMVP
   .\uninstall-service.ps1
   .\install-service.ps1
   ```

---

## 7. Sorun Giderme

### Sayfa açılmıyor / “Güvenli bağlantı sağlayamıyor” (ERR_SSL_PROTOCOL_ERROR)

- Service modunda **http://localhost:50201** kullanın (https değil).  
- Servisi yeniden başlatın: `Restart-Service -Name CallTrackMVP`

### Port 50201 kullanımda

**Kurulum.bat** ile kurduysanız port zaten otomatik 50202’ye alınmış olabilir; tarayıcıda **http://localhost:50202** deneyin.

Manuel kurulum yaptıysanız veya portu sonradan değiştirmek istiyorsanız, **sadece Publish klasörüne sahipseniz** portu ortam değişkeni ile değiştirin (kod veya solution gerekmez):

1. **Yeni portu seçin** (örn. 50202 veya 50500). Boş olduğundan emin olun.
2. **Sistem ortam değişkeni ekleyin** — Yönetici PowerShell:
   ```powershell
   [Environment]::SetEnvironmentVariable("ASPNETCORE_URLS", "http://0.0.0.0:50202", "Machine")
   ```
   (Örnekte port 50202; istediğiniz portu yazın.)
3. **Servisi yeniden başlatın** (ortam değişkeni servis hesabına yansısın diye):
   ```powershell
   Restart-Service -Name CallTrackMVP
   ```
4. Tarayıcıda artık **http://localhost:50202** (veya seçtiğiniz port) kullanın. LAN’dan erişimde de aynı portu kullanın ve güvenlik duvarında bu portu açın.

### Veritabanı kilitli (SQLite locked)

- Aynı makinede tek bir CallTrack MVP örneği çalıştığından emin olun.  
- Service’i durdurup tekrar başlatın.

### Servis kurulurken hata

- PowerShell’i **Yönetici olarak** çalıştırın.  
- Script’i **Publish klasörü içinden** çalıştırın (CallTrackMVP.Web.exe ile aynı dizin).  
- `.\install-service.ps1` çalıştırma ilkesi engelliyorsa: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

---

## 8. Özet – Hızlı Başvuru

| Konu | Değer |
|------|--------|
| **Tek paket kurulum** | **CallTrackMVP_Setup.exe**’ye çift tıklayın (yönetici izni verin); tek dosya, .NET gerekmez |
| **Publish + Kurulum.bat** | Publish içindeki **Kurulum.bat**’e çift tıklayın; C:\CallTrackMVP oluşturulur, .NET 8 Runtime gerekir |
| Service adı | CallTrackMVP |
| Görünen ad | CallTrack MVP - Çağrı Takip |
| Service erişim adresi | http://localhost:50201 (port meşgulse 50202) |
| Kurulum klasörü | C:\CallTrackMVP (Kurulum.bat her zaman bu klasörü oluşturup dosyaları oraya kopyalar) |
| Veritabanı dosyası | `Data\CallTrack.db` (exe yanında) |
| Varsayılan admin | admin / Admin123! |

Bu kılavuz, kurulum ve varsayılan ayarlar için tek referans olarak kullanılabilir. Özel ortamlar için `appsettings.json` ve ortam değişkenleri ile ayarlar değiştirilebilir.
