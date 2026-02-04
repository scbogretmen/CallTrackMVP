# 02 – Kod Referansları

Bu dosya, CallTrack MVP projesindeki önemli sınıflar, route’lar ve kod konumları için hızlı referans sağlar.

---

## Route ve URL Haritası

| Route | Controller | Action | Yetki |
|-------|------------|--------|-------|
| `/` | Home | Index | Authorize |
| `/Home/Error` | Home | Error | AllowAnonymous |
| `/Auth/Login` | Auth | Login (GET/POST) | Anonymous |
| `/Auth/Logout` | Auth | Logout (POST) | Authorize |
| `/CallLogs` | CallLogs | Index | Authorize |
| `/CallLogs/Details/{id}` | CallLogs | Details | Authorize |
| `/CallLogs/Create` | CallLogs | Create (GET/POST) | Authorize |
| `/CallLogs/Edit/{id}` | CallLogs | Edit (GET/POST) | Authorize |
| `/CallLogs/Delete/{id}` | CallLogs | Delete / DeleteConfirmed | Authorize |
| `/CallLogs/AcknowledgeUpdates/{id}` | CallLogs | AcknowledgeUpdates (POST) | Authorize |
| `/Admin/Reports` | Admin | Reports | Admin |
| `/Admin/ExportExcel` | Admin | ExportExcel (POST) | Admin |
| `/Admin/Users` | Admin | Users | Admin |
| `/Admin/CreateUser` | Admin | CreateUser (GET/POST) | Admin |
| `/Admin/DeleteUser/{id}` | Admin | DeleteUser / DeleteUserConfirmed | Admin |
| `/Admin/CallTypes` | Admin | CallTypes | Admin |
| `/Admin/CreateCallType` | Admin | CreateCallType (GET/POST) | Admin |
| `/Admin/DeleteCallType/{id}` | Admin | DeleteCallType / DeleteCallTypeConfirmed | Admin |

---

## Controllers

### HomeController
**Dosya:** `CallTrackMVP.Web/Controllers/HomeController.cs`

| Metod | Açıklama |
|-------|----------|
| `Index` | Ana sayfa; bugün/hafta çağrı sayıları. User kendi kayıtlarını görür, Admin tümünü görür. |
| `Error` | Hata sayfası (AllowAnonymous). |

### AuthController
**Dosya:** `CallTrackMVP.Web/Controllers/AuthController.cs`

| Metod | Açıklama |
|-------|----------|
| `Login` (GET/POST) | Giriş sayfası. Cookie auth ile Claims oluşturur (NameIdentifier, Name, GivenName, Role). |
| `Logout` (POST) | Oturumu kapatır. |

**Önemli:** `PasswordHasher<AppUser>` kullanılır; şema adı `CookieAuth`.

### CallLogsController
**Dosya:** `CallTrackMVP.Web/Controllers/CallLogsController.cs`

| Metod | Açıklama |
|-------|----------|
| `Index` | Çağrı listesi. Okunmamış güncellemeler `ViewBag.UnreadCallLogIds` ile gönderilir. |
| `Details` | Detay görüntüleme. `ViewBag.HasUnreadUpdates` ile okunmamış güncelleme bilgisi. |
| `AcknowledgeUpdates` | Kullanıcının güncellemeleri okuduğunu işaretler (CallLogAcknowledgment). |
| `Create` (GET/POST) | Yeni çağrı. `SiraNo` otomatik atanır; `Tarih`+`CagriNo` benzersiz kontrolü yapılır. |
| `Edit` (GET/POST) | Düzenleme. İsteğe bağlı güncelleme ekleme (GuncellenenTarih, GuncellenenCagriSaat, GuncellemeNedeni). |
| `Delete` / `DeleteConfirmed` | Silme. |

**Yardımcı:** `GetCurrentUserId()`, `IsAdmin`, `GetFilteredQuery()` – User rolünde kendi kayıtları filtreler.

### AdminController
**Dosya:** `CallTrackMVP.Web/Controllers/AdminController.cs`

| Metod | Açıklama |
|-------|----------|
| `Reports` | Rapor sayfası. Tarih, teknisyen, çağrı türü, çağrı no ile filtreleme. |
| `ExportExcel` | Filtrelenmiş kayıtları Excel’e dışa aktarır (`IExcelExportService`). |
| `Users` | Kullanıcı listesi. |
| `CreateUser` | Yeni kullanıcı (Admin/User rolü). |
| `DeleteUser` | Kullanıcı silme; kendini silme ve son Admin silme engellenir. |
| `CallTypes` | Çağrı tipleri listesi. |
| `CreateCallType` | Yeni çağrı tipi. |
| `DeleteCallType` | Çağrı tipi silme; kullanımda ise silinemez. |

---

## Models

### Entity Modelleri

| Model | Dosya | Açıklama |
|-------|-------|----------|
| **AppUser** | `Models/AppUser.cs` | Id, FullName, UserName, PasswordHash, Role (Admin/User). Navigation: CallLogs. |
| **CallLog** | `Models/CallLog.cs` | SiraNo, CagriNo, CagriTuru, TeknisyenAdi, Tarih, MevcutCagriSaat, GuncellenenTarih/Saat/Nedeni, CreatedByUserId. Navigation: CreatedByUser, Updates. |
| **CallType** | `Models/CallType.cs` | Id, Name. |
| **CallLogUpdate** | `Models/CallLogUpdate.cs` | CallLogId, GuncellenenTarih, GuncellenenCagriSaat, GuncellemeNedeni, CreatedAt. |
| **CallLogAcknowledgment** | `Models/CallLogAcknowledgment.cs` | UserId, CallLogId, LastAcknowledgedUpdateId (composite PK). |

### ViewModels

| ViewModel | Dosya | Kullanım |
|-----------|-------|----------|
| **LoginViewModel** | `Models/ViewModels/LoginViewModel.cs` | Auth/Login (UserName, Password, RememberMe) |
| **CreateUserViewModel** | `Models/ViewModels/CreateUserViewModel.cs` | Admin/CreateUser |
| **UserListViewModel** | `Models/ViewModels/UserListViewModel.cs` | Admin/Users listesi |
| **ReportsFilterViewModel** | `Models/ViewModels/ReportsFilterViewModel.cs` | Admin/Reports filtreleri |

---

## Data

### AppDbContext
**Dosya:** `Data/AppDbContext.cs`

| DbSet | Tablo |
|-------|-------|
| `AppUsers` | AppUsers |
| `CallLogs` | CallLogs |
| `CallTypes` | CallTypes |
| `CallLogUpdates` | CallLogUpdates |
| `CallLogAcknowledgments` | CallLogAcknowledgments |

**Indexler:** AppUser(UserName), CallType(Name), CallLog(Tarih, CagriNo) unique.

### DbInitializer
**Dosya:** `Data/DbInitializer.cs`

| Metod | Açıklama |
|-------|----------|
| `EnsureCallLogUpdatesTableExistsAsync` | CallLogUpdates tablosu yoksa oluşturur. |
| `EnsureCallTypesAsync` | CallTypes tablosu ve varsayılan tipler (Web, Telefon, Eposta). |
| `EnsureCallLogAcknowledgmentsTableExistsAsync` | CallLogAcknowledgments tablosu. |
| `InitializeAsync` | Varsayılan kullanıcılar (admin, ahmet, ayse, kubra, mehmet). |

---

## Services

### IExcelExportService / ExcelExportService
**Dosya:** `Services/IExcelExportService.cs`, `Services/ExcelExportService.cs`

| Metod | Açıklama |
|-------|----------|
| `ExportCallLogsToExcel(IEnumerable<CallLog> logs)` | CallLog listesini Excel (ClosedXML) ile byte[] olarak döner. |

**DI:** `AddScoped<IExcelExportService, ExcelExportService>` (Program.cs).

---

## Views

| Klasör | View | İlişkili Action |
|--------|------|-----------------|
| Home | Index, Error | Home/Index, Home/Error |
| Auth | Login | Auth/Login |
| CallLogs | Index, Details, Create, Edit, Delete | CallLogs/* |
| Admin | Reports, Users, CreateUser, DeleteUser, CallTypes, CreateCallType, DeleteCallType | Admin/* |
| Shared | _Layout, _ValidationScriptsPartial | Layout / validation script’leri |

---

## Program.cs Yapılandırma Noktaları

| Satır / Bölüm | Açıklama |
|---------------|----------|
| `ContentRootPath` | Uygulama çalışma dizini (Service için exe klasörü). |
| `UseWindowsService()` | Windows Service desteği. |
| `Data\CallTrack.db` | SQLite bağlantı dizesi. |
| `UseUrls` | Service: `http://0.0.0.0:50201`, Geliştirme: `https://0.0.0.0:50201`. |
| `CookieAuth` | LoginPath: /Auth/Login, ExpireTimeSpan: 8 saat, SlidingExpiration: true. |
| `UseHttpsRedirection` | Sadece geliştirme modunda (Service’te kapatılmış). |
| Migration + DbInitializer | Uygulama başlangıcında veritabanı migrate ve seed. |

---

## Claims Kullanımı

Oturumda saklanan Claim’ler:

- `ClaimTypes.NameIdentifier` → User Id (int)
- `ClaimTypes.Name` → UserName
- `ClaimTypes.GivenName` → FullName
- `ClaimTypes.Role` → "Admin" veya "User"

Rol kontrolü: `User.IsInRole("Admin")`, `[Authorize(Roles = "Admin")]`.
