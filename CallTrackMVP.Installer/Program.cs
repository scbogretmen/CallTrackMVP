using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Principal;

const string HedefKlasor = @"C:\CallTrackMVP";

// Yonetici degilse kendini yonetici olarak yeniden baslat
if (!IsRunAsAdministrator())
{
    Console.WriteLine("Yonetici izni isteniyor...");
    var exe = Environment.ProcessPath ?? AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar + "CallTrackMVP_Setup.exe";
    try
    {
        var start = new ProcessStartInfo
        {
            FileName = exe!,
            UseShellExecute = true,
            Verb = "runas"
        };
        Process.Start(start);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Hata: " + ex.Message);
        Console.WriteLine("Bu programi sag tiklayip 'Yonetici olarak calistir' ile acin.");
    }
    return 1;
}

Console.WriteLine("CallTrack MVP kurulumu basliyor...");

// Gomulu payload.zip'i bul ve C:\CallTrackMVP'e cikart
var asm = Assembly.GetExecutingAssembly();
var resourceName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("payload.zip", StringComparison.OrdinalIgnoreCase));
if (string.IsNullOrEmpty(resourceName))
{
    Console.WriteLine("HATA: Kurulum paketi (payload) bulunamadi. CallTrackMVP_Setup.exe bozuk veya eksik olabilir.");
    Bekle();
    return 1;
}

Directory.CreateDirectory(HedefKlasor);
string? tempZip = null;
try
{
    tempZip = Path.Combine(Path.GetTempPath(), "CallTrackMVP_payload_" + Guid.NewGuid().ToString("N") + ".zip");
    using (var stream = asm.GetManifestResourceStream(resourceName)!)
    using (var file = File.Create(tempZip))
        stream.CopyTo(file);

    ZipFile.ExtractToDirectory(tempZip, HedefKlasor, overwriteFiles: true);
    Console.WriteLine("Dosyalar " + HedefKlasor + " konumuna cikartildi.");
}
catch (Exception ex)
{
    Console.WriteLine("Cikartma hatasi: " + ex.Message);
    Bekle();
    return 1;
}
finally
{
    if (tempZip != null && File.Exists(tempZip))
        try { File.Delete(tempZip); } catch { }
}

// Kurulum script'ini calistir (servis, port, guvenlik duvari, tarayici)
var kurulumPs1 = Path.Combine(HedefKlasor, "Kurulum.ps1");
if (File.Exists(kurulumPs1))
{
    Console.WriteLine("Servis ve ayarlar yapilandiriliyor...");
    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{kurulumPs1}\"",
            WorkingDirectory = HedefKlasor,
            UseShellExecute = false,
            CreateNoWindow = false
        };
        using var p = Process.Start(psi);
        p?.WaitForExit();
        if (p != null && p.ExitCode != 0)
        {
            Console.WriteLine("Kurulum scripti hata kodu ile cikti: " + p.ExitCode);
            Bekle();
            return 1;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Script calistirma hatasi: " + ex.Message);
        Bekle();
        return 1;
    }
}
else
{
    Console.WriteLine("UYARI: Kurulum.ps1 bulunamadi. Servisi manuel kurun: " + HedefKlasor + " icinde install-service.ps1 calistirin.");
}

Console.WriteLine();
Console.WriteLine("Kurulum tamamlandi.");
Bekle();
return 0;

[SupportedOSPlatform("windows")]
static bool IsRunAsAdministrator()
{
    try
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    catch { return false; }
}

static void Bekle()
{
    Console.WriteLine();
    Console.WriteLine("Cikmak icin Enter'a basin...");
    Console.ReadLine();
}
