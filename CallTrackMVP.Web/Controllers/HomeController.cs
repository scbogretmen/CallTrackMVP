using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CallTrackMVP.Web.Data;

namespace CallTrackMVP.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

        var query = _db.CallLogs.AsQueryable();
        if (!isAdmin && userId.HasValue)
            query = query.Where(c => c.CreatedByUserId == userId.Value);

        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday)
            weekStart = weekStart.AddDays(-7);
        var weekEnd = weekStart.AddDays(6);

        var todayCount = await query.Where(c => c.Tarih.Date == today).CountAsync(cancellationToken);
        var weekCount = await query
            .Where(c => c.Tarih.Date >= weekStart && c.Tarih.Date <= weekEnd)
            .CountAsync(cancellationToken);

        ViewBag.TodayCount = todayCount;
        ViewBag.WeekCount = weekCount;
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}
