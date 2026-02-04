using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CallTrackMVP.Web.Data;
using CallTrackMVP.Web.Models;
using CallTrackMVP.Web.Models.ViewModels;
using CallTrackMVP.Web.Services;

namespace CallTrackMVP.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly IExcelExportService _excelExport;
    private readonly PasswordHasher<AppUser> _hasher = new();

    public AdminController(AppDbContext db, IExcelExportService excelExport)
    {
        _db = db;
        _excelExport = excelExport;
    }

    public async Task<IActionResult> Reports(ReportsFilterViewModel? filter, CancellationToken cancellationToken = default)
    {
        filter ??= new ReportsFilterViewModel();
        var query = _db.CallLogs.Include(c => c.CreatedByUser).Include(c => c.Updates).AsQueryable();

        if (filter.TarihBaslangic.HasValue)
            query = query.Where(c => c.Tarih.Date >= filter.TarihBaslangic.Value.Date);
        if (filter.TarihBitis.HasValue)
            query = query.Where(c => c.Tarih.Date <= filter.TarihBitis.Value.Date);
        if (!string.IsNullOrWhiteSpace(filter.TeknisyenAdi))
            query = query.Where(c => c.TeknisyenAdi.Contains(filter.TeknisyenAdi.Trim()));
        if (!string.IsNullOrWhiteSpace(filter.CagriTuru))
            query = query.Where(c => c.CagriTuru == filter.CagriTuru.Trim());
        if (!string.IsNullOrWhiteSpace(filter.CagriNo))
            query = query.Where(c => c.CagriNo.Contains(filter.CagriNo.Trim()));

        var items = await query
            .OrderByDescending(c => c.Tarih)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        ViewBag.Filter = filter;
        ViewBag.Items = items;
        ViewBag.CallTypes = await _db.CallTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
        return View(filter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportExcel(ReportsFilterViewModel filter, CancellationToken cancellationToken = default)
    {
        var query = _db.CallLogs.Include(c => c.CreatedByUser).Include(c => c.Updates).AsQueryable();

        if (filter.TarihBaslangic.HasValue)
            query = query.Where(c => c.Tarih.Date >= filter.TarihBaslangic.Value.Date);
        if (filter.TarihBitis.HasValue)
            query = query.Where(c => c.Tarih.Date <= filter.TarihBitis.Value.Date);
        if (!string.IsNullOrWhiteSpace(filter.TeknisyenAdi))
            query = query.Where(c => c.TeknisyenAdi.Contains(filter.TeknisyenAdi.Trim()));
        if (!string.IsNullOrWhiteSpace(filter.CagriTuru))
            query = query.Where(c => c.CagriTuru == filter.CagriTuru.Trim());
        if (!string.IsNullOrWhiteSpace(filter.CagriNo))
            query = query.Where(c => c.CagriNo.Contains(filter.CagriNo.Trim()));

        var items = await query
            .OrderByDescending(c => c.Tarih)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var bytes = _excelExport.ExportCallLogsToExcel(items);
        var fileName = $"CagriKayitlari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Users(CancellationToken cancellationToken = default)
    {
        var users = await _db.AppUsers
            .OrderBy(u => u.UserName)
            .Select(u => new UserListViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                UserName = u.UserName,
                Role = u.Role,
                CallLogCount = u.CallLogs.Count
            })
            .ToListAsync(cancellationToken);
        return View(users);
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        return View(new CreateUserViewModel { Role = "User" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _db.AppUsers.AnyAsync(u => u.UserName == model.UserName, cancellationToken))
            ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı zaten kullanılıyor.");

        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Role = model.Role
            };
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
            return RedirectToAction(nameof(Users));
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken = default)
    {
        var user = await _db.AppUsers
            .Include(u => u.CallLogs)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            return NotFound();
        ViewBag.CanDelete = user.CallLogs.Count == 0;
        ViewBag.CurrentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;
        ViewBag.AdminCount = await _db.AppUsers.CountAsync(u => u.Role == "Admin", cancellationToken);
        return View(user);
    }

    [HttpPost, ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int id, CancellationToken cancellationToken = default)
    {
        var user = await _db.AppUsers
            .Include(u => u.CallLogs)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            return NotFound();

        var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;
        if (user.Id == currentUserId)
        {
            TempData["Error"] = "Kendinizi silemezsiniz.";
            return RedirectToAction(nameof(Users));
        }

        if (user.Role == "Admin")
        {
            var adminCount = await _db.AppUsers.CountAsync(u => u.Role == "Admin", cancellationToken);
            if (adminCount <= 1)
            {
                TempData["Error"] = "Son Admin kullanıcı silinemez.";
                return RedirectToAction(nameof(Users));
            }
        }

        if (user.CallLogs.Count > 0)
        {
            TempData["Error"] = "Bu kullanıcının çağrı kayıtları var. Önce kayıtları silin veya başka kullanıcıya atayın.";
            return RedirectToAction(nameof(Users));
        }

        _db.AppUsers.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Kullanıcı silindi.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> CallTypes(CancellationToken cancellationToken = default)
    {
        var types = await _db.CallTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
        return View(types);
    }

    [HttpGet]
    public IActionResult CreateCallType() => View(new CallType());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCallType(CallType model, CancellationToken cancellationToken = default)
    {
        var name = model.Name?.Trim() ?? "";
        if (string.IsNullOrEmpty(name))
            ModelState.AddModelError(nameof(model.Name), "Çağrı türü adı zorunludur.");
        else if (await _db.CallTypes.AnyAsync(t => t.Name == name, cancellationToken))
            ModelState.AddModelError(nameof(model.Name), "Bu çağrı türü zaten mevcut.");

        if (ModelState.IsValid)
        {
            _db.CallTypes.Add(new CallType { Name = name });
            await _db.SaveChangesAsync(cancellationToken);
            TempData["Success"] = "Çağrı türü eklendi.";
            return RedirectToAction(nameof(CallTypes));
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteCallType(int id, CancellationToken cancellationToken = default)
    {
        var type = await _db.CallTypes.FindAsync(new object[] { id }, cancellationToken);
        if (type == null)
            return NotFound();
        var inUse = await _db.CallLogs.AnyAsync(c => c.CagriTuru == type.Name, cancellationToken);
        ViewBag.InUse = inUse;
        return View(type);
    }

    [HttpPost, ActionName("DeleteCallType")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCallTypeConfirmed(int id, CancellationToken cancellationToken = default)
    {
        var type = await _db.CallTypes.FindAsync(new object[] { id }, cancellationToken);
        if (type == null)
            return NotFound();
        if (await _db.CallLogs.AnyAsync(c => c.CagriTuru == type.Name, cancellationToken))
        {
            TempData["Error"] = "Bu çağrı türü kullanılıyor, silinemez.";
            return RedirectToAction(nameof(CallTypes));
        }
        _db.CallTypes.Remove(type);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Çağrı türü silindi.";
        return RedirectToAction(nameof(CallTypes));
    }
}
