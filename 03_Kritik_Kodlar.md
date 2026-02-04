# 03 – Kritik Kodlar

Bu dosya, güvenlik, iş kuralları ve sistem davranışı için kritik olan kod bölümlerini referans olarak listeler. Değişiklik yaparken bu noktaları gözden geçirin.

---

## 1. Uygulama Başlangıcı ve Yapılandırma

**Dosya:** `CallTrackMVP.Web/Program.cs`

### Çalışma dizini (Windows Service)

```csharp
var contentRoot = AppContext.BaseDirectory;
Directory.SetCurrentDirectory(contentRoot);
```

Service, exe’nin bulunduğu klasörü (örn. C:\CallTrackMVP) çalışma dizini yapar; `Data\CallTrack.db` ve appsettings bu dizine göre çözülür.

### HTTP/HTTPS ve port

```csharp
var useHttp = OperatingSystem.IsWindows() && !Environment.UserInteractive;
builder.WebHost.UseUrls(useHttp ? "http://0.0.0.0:50201" : "https://0.0.0.0:50201");
```

- **Service:** HTTP, port 50201 (sertifika yok).
- **Geliştirme:** HTTPS, port 50201.

### HTTPS yönlendirmesi

```csharp
if (!useHttp)
    app.UseHttpsRedirection();
```

Service modunda `UseHttpsRedirection` kapalı; açık olsaydı tarayıcı HTTPS’e yönlenir ve ERR_SSL_PROTOCOL_ERROR oluşur.

### Kimlik doğrulama şeması

```csharp
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
```

Şema adı **CookieAuth**; SignIn/SignOut ve `[Authorize]` bu şemayı kullanır.

---

## 2. Kimlik Doğrulama (Giriş)

**Dosya:** `CallTrackMVP.Web/Controllers/AuthController.cs`

### Şifre doğrulama

```csharp
var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
if (result == PasswordVerificationResult.Failed)
{
    ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
    return View(model);
}
```

- Kullanıcı bulunamazsa veya şifre yanlışsa **aynı** mesaj verilir (kullanıcı adı tahminini zorlaştırır).
- `PasswordHasher<AppUser>` (Microsoft.AspNetCore.Identity) kullanılır.

### returnUrl güvenliği

```csharp
if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
    return Redirect(returnUrl);
return RedirectToAction("Index", "Home");
```

Sadece **yerel** URL’lere yönlendirilir; açık uygulama içi yönlendirme (open redirect) riski azaltılır.

### Claims oluşturma

```csharp
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new(ClaimTypes.Name, user.UserName),
    new(ClaimTypes.GivenName, user.FullName),
    new(ClaimTypes.Role, user.Role)
};
```

Rol ve kullanıcı kimliği bu claim’lerden okunur; `[Authorize(Roles = "Admin")]` ve `GetCurrentUserId()` buna dayanır.

---

## 3. Yetkilendirme ve Veri Filtreleme

**Dosya:** `CallTrackMVP.Web/Controllers/CallLogsController.cs`

### User rolü: Sadece kendi kayıtları

```csharp
private IQueryable<CallLog> GetFilteredQuery()
{
    IQueryable<CallLog> query = _db.CallLogs.Include(c => c.CreatedByUser);
    if (!IsAdmin)
    {
        var uid = GetCurrentUserId();
        if (uid.HasValue)
            query = query.Where(c => c.CreatedByUserId == uid.Value);
    }
    return query;
}
```

Tüm listeleme, detay, düzenleme ve silme **GetFilteredQuery()** ile yapılır; User sadece `CreatedByUserId == kendi Id` olan kayıtları görür ve değiştirebilir.

### Kullanıcı kimliği

```csharp
private int? GetCurrentUserId()
{
    var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    return int.TryParse(claim, out var id) ? id : null;
}
```

Claim yoksa veya parse edilemezse `null` döner; Create’te `userId` yoksa kayıt oluşturulmaz.

---

## 4. İş Kuralları

### Tarih + Çağrı No benzersizliği

**Dosya:** `CallTrackMVP.Web/Controllers/CallLogsController.cs`

**Create (POST):**

```csharp
if (await _db.CallLogs.AnyAsync(c => c.Tarih.Date == model.Tarih && c.CagriNo == model.CagriNo, cancellationToken))
    ModelState.AddModelError(nameof(model.CagriNo), "Bu tarih ve çağrı no kombinasyonu zaten mevcut.");
```

**Edit (POST):**

```csharp
var duplicate = await _db.CallLogs.AnyAsync(c =>
    c.Tarih.Date == model.Tarih && c.CagriNo == model.CagriNo && c.Id != id, cancellationToken);
if (duplicate)
    ModelState.AddModelError(nameof(model.CagriNo), "Bu tarih ve çağrı no kombinasyonu zaten mevcut.");
```

Veritabanında da **AppDbContext** içinde `CallLog` için `(Tarih, CagriNo)` unique index tanımlı; çift kayıt engellenir.

### Kullanıcı silme kuralları

**Dosya:** `CallTrackMVP.Web/Controllers/AdminController.cs` – `DeleteUserConfirmed`

| Kural | Kod / Kontrol |
|-------|----------------|
| Kendini silemez | `user.Id == currentUserId` → TempData["Error"], redirect |
| Son Admin silinemez | `user.Role == "Admin"` ve `adminCount <= 1` → TempData["Error"], redirect |
| Çağrı kaydı varsa silinemez | `user.CallLogs.Count > 0` → TempData["Error"], redirect |

Bu kontroller olmadan son admin kaldırılabilir veya veri bütünlüğü bozulabilir.

### Çağrı tipi silme

**Dosya:** `CallTrackMVP.Web/Controllers/AdminController.cs` – `DeleteCallTypeConfirmed`

```csharp
if (await _db.CallLogs.AnyAsync(c => c.CagriTuru == type.Name, cancellationToken))
{
    TempData["Error"] = "Bu çağrı türü kullanılıyor, silinemez.";
    return RedirectToAction(nameof(CallTypes));
}
```

Kullanımda olan çağrı tipi silinmez; referans bütünlüğü korunur.

---

## 5. Veritabanı: Index ve İlişkiler

**Dosya:** `CallTrackMVP.Web/Data/AppDbContext.cs` – `OnModelCreating`

| Entity | Kritik ayar |
|--------|-------------|
| **AppUser** | `UserName` unique index |
| **CallType** | `Name` unique index |
| **CallLog** | `(Tarih, CagriNo)` unique index; `CreatedByUser` Restrict; `Updates` Cascade |
| **CallLogAcknowledgment** | Composite PK `(UserId, CallLogId)`; User ve CallLog için Cascade delete |

Bu index’ler ve delete davranışları değiştirilirse migration gerekir ve mevcut veri/performans etkilenir.

---

## 6. Varsayılan Kullanıcılar ve Şifreler (Seed)

**Dosya:** `CallTrackMVP.Web/Data/DbInitializer.cs` – `InitializeAsync`

```csharp
var users = new[]
{
    new AppUser { UserName = "admin", FullName = "Yönetici", Role = "Admin" },
    // ...
};
var passwords = new[] { "Admin123!", "Ahmet123!", "Ayse123!", "Kubra123!", "Mehmet123!" };
```

- Sadece **hiç kullanıcı yoksa** çalışır (`if (await db.AppUsers.AnyAsync()) return;`).
- Varsayılan şifreler **dokümante edilmeli** (KURULUM_KILAVUZU.md) ve ilk girişten sonra değiştirilmeli.
- Bu liste veya şifreler değişirse yalnızca **yeni kurulumlar** etkilenir; mevcut veritabanında zaten kullanıcı varsa seed atlanır.

---

## 7. AntiForgeryToken

Tüm veri değiştiren POST aksiyonları `[ValidateAntiForgeryToken]` ile işaretlenmiştir:

- Auth: Login, Logout  
- CallLogs: Create, Edit, Delete, AcknowledgeUpdates  
- Admin: ExportExcel, CreateUser, DeleteUser, CreateCallType, DeleteCallType  

Form’da `@Html.AntiForgeryToken()` veya ilgili token kullanılmazsa istek 400 ile reddedilir; CSRF riski azaltılır.

---

## Özet Kontrol Listesi

| Konu | Nerede | Dikkat |
|------|--------|--------|
| Service çalışma dizini | Program.cs | ContentRootPath ve Data yolu |
| HTTP/HTTPS ve port | Program.cs | useHttp, UseUrls, UseHttpsRedirection |
| Cookie şeması adı | Program.cs, AuthController | "CookieAuth" tutarlı kullanılmalı |
| returnUrl | AuthController | Sadece Url.IsLocalUrl |
| User veri filtresi | CallLogsController.GetFilteredQuery | Admin dışında CreatedByUserId |
| Tarih + CagriNo unique | CallLogsController, AppDbContext | Create/Edit ve unique index |
| Admin/kullanıcı silme | AdminController.DeleteUserConfirmed | Kendini silme, son admin, çağrı kaydı |
| Çağrı tipi silme | AdminController.DeleteCallTypeConfirmed | Kullanımda mı kontrolü |
| Seed şifreleri | DbInitializer.InitializeAsync | Sadece boş DB; dokümante edilmeli |
| POST aksiyonları | Tüm controller’lar | ValidateAntiForgeryToken |
