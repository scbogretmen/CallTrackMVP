using Microsoft.EntityFrameworkCore;
using CallTrackMVP.Web.Data;
using CallTrackMVP.Web.Services;

// Windows Service: çalışma dizini executable klasörü olmalı (System32 değil)
var contentRoot = AppContext.BaseDirectory;
Directory.SetCurrentDirectory(contentRoot);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot
});
builder.Host.UseWindowsService(); // Windows Service olarak çalıştırma desteği

// Ensure Data folder exists for SQLite
var dataPath = Path.Combine(builder.Environment.ContentRootPath, "Data");
if (!Directory.Exists(dataPath))
{
    Directory.CreateDirectory(dataPath);
}

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Cookie authentication
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Kestrel: Service/non-interactive ise HTTP (sertifika yok), değilse HTTPS
var useHttp = OperatingSystem.IsWindows() && !Environment.UserInteractive;
builder.WebHost.UseUrls(useHttp ? "http://0.0.0.0:50201" : "https://0.0.0.0:50201");

var app = builder.Build();

// Initialize database and seed users
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    // CallLogUpdates tablosu yoksa oluştur (migration atlandıysa)
    await DbInitializer.EnsureCallLogUpdatesTableExistsAsync(db);
    await DbInitializer.EnsureCallTypesAsync(db);
    await DbInitializer.EnsureCallLogAcknowledgmentsTableExistsAsync(db);
    await DbInitializer.InitializeAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Service modunda sadece HTTP dinlendiği için HTTPS yönlendirmesi yapma (yoksa ERR_SSL_PROTOCOL_ERROR)
if (!useHttp)
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
