# CallTrack MVP – Güncellemeler

Bu dosya, CallTrack MVP (Çağrı Takip) uygulamasına yapılan güncellemeleri ve sürüm notlarını listeler.

---

## 2025-02-04

### Kurulum ve dağıtım

- **Tek paket kurulum (CallTrackMVP_Setup.exe)**  
  - Tek EXE ile kurulum eklendi. Kullanıcı sadece bu dosyayı çalıştırır; .NET ayrıca gerekmez.  
  - Üretmek için proje kökünde: `.\build-installer.ps1`  
  - Çıktı: `CallTrackMVP.Installer\bin\SetupOutput\CallTrackMVP_Setup.exe`

- **Kurulum script’leri (Kurulum.ps1 / Kurulum.bat)**  
  - Proje nerede olursa olsun **C:\CallTrackMVP** oluşturulup tüm dosyalar oraya kopyalanıyor.  
  - Port 50201 meşgulse otomatik 50202 kullanılıyor.  
  - Güvenlik duvarı kuralı ve Windows servisi otomatik ekleniyor.  
  - Kurulum.bat çift tıklanınca yönetici izni isteniyor, kurulum tek adımda tamamlanıyor.

- **Kurulum kılavuzu (KURULUM_KILAVUZU.md)**  
  - Tek paket EXE, Publish + Kurulum.bat ve manuel kurulum adımları eklendi.  
  - Varsayılan ayarlar (port, veritabanı, kullanıcılar, çağrı tipleri) dokümante edildi.  
  - Sorun giderme ve hızlı başvuru bölümleri güncellendi.

### Uygulama

- **Service modunda HTTPS yönlendirmesi**  
  - Windows Service olarak çalışırken sadece HTTP dinlendiği için HTTPS yönlendirmesi kapatıldı.  
  - Böylece tarayıcıda **http://localhost:50201** kullanıldığında "Bu site güvenli bağlantı sağlayamıyor" (ERR_SSL_PROTOCOL_ERROR) hatası oluşmuyor.

---

## Gelecek güncellemeler

Buraya yeni tarih ve maddeler ekleyebilirsiniz. Örnek:

- **YYYY-AA-GG**
  - Değişiklik özeti.
  - İsteğe bağlı detay.
