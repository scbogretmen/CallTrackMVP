using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CallTrackMVP.Web.Data;
using CallTrackMVP.Web.Models;

namespace CallTrackMVP.Web.Controllers;

[Authorize]
public class CallLogsController : Controller
{
    private readonly AppDbContext _db;

    public CallLogsController(AppDbContext db)
    {
        _db = db;
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }

    private bool IsAdmin => User.IsInRole("Admin");

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

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var items = await GetFilteredQuery()
            .Include(c => c.Updates)
            .OrderByDescending(c => c.Tarih)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var unreadIds = new HashSet<int>();
        if (userId.HasValue)
        {
            var acks = await _db.CallLogAcknowledgments
                .Where(a => a.UserId == userId.Value && items.Select(x => x.Id).Contains(a.CallLogId))
                .ToDictionaryAsync(a => a.CallLogId, a => a.LastAcknowledgedUpdateId, cancellationToken);
            foreach (var item in items)
            {
                if (item.Updates == null || !item.Updates.Any()) continue;
                var maxUpdateId = item.Updates.Max(u => u.Id);
                if (!acks.TryGetValue(item.Id, out var lastAck) || lastAck < maxUpdateId)
                    unreadIds.Add(item.Id);
            }
        }
        ViewBag.UnreadCallLogIds = unreadIds;
        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var item = await GetFilteredQuery()
            .Include(c => c.Updates.OrderByDescending(u => u.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (item == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var hasUnread = false;
        if (userId.HasValue && item.Updates?.Any() == true)
        {
            var maxUpdateId = item.Updates.Max(u => u.Id);
            var ack = await _db.CallLogAcknowledgments.FindAsync(new object[] { userId.Value, id }, cancellationToken);
            hasUnread = ack == null || ack.LastAcknowledgedUpdateId < maxUpdateId;
        }
        ViewBag.HasUnreadUpdates = hasUnread;
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcknowledgeUpdates(int id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction(nameof(Details), new { id });

        var item = await GetFilteredQuery()
            .Include(c => c.Updates)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (item == null)
            return NotFound();

        var maxUpdateId = item.Updates?.Any() == true ? item.Updates.Max(u => u.Id) : 0;
        var ack = await _db.CallLogAcknowledgments.FindAsync(new object[] { userId.Value, id }, cancellationToken);
        if (ack == null)
            _db.CallLogAcknowledgments.Add(new CallLogAcknowledgment { UserId = userId.Value, CallLogId = id, LastAcknowledgedUpdateId = maxUpdateId });
        else
            ack.LastAcknowledgedUpdateId = Math.Max(ack.LastAcknowledgedUpdateId, maxUpdateId);
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        ViewBag.CallTypes = await _db.CallTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
        var nextSiraNo = await _db.CallLogs.MaxAsync(c => (int?)c.SiraNo, cancellationToken) ?? 0;
        nextSiraNo++;
        return View(new CallLog
        {
            Tarih = DateTime.Today,
            MevcutCagriSaat = DateTime.Now.ToString("HH:mm"),
            SiraNo = nextSiraNo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CallLog model, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue || userId == 0)
        {
            ModelState.AddModelError(string.Empty, "Oturum bulunamadı. Lütfen tekrar giriş yapın.");
            return View(model);
        }

        model.Tarih = model.Tarih.Date;
        model.CreatedAt = DateTime.Now;
        model.CreatedByUserId = userId.Value;
        model.SiraNo = await _db.CallLogs.MaxAsync(c => (int?)c.SiraNo, cancellationToken) ?? 0;
        model.SiraNo++;

        if (await _db.CallLogs.AnyAsync(c => c.Tarih.Date == model.Tarih && c.CagriNo == model.CagriNo, cancellationToken))
            ModelState.AddModelError(nameof(model.CagriNo), "Bu tarih ve çağrı no kombinasyonu zaten mevcut.");

        if (ModelState.IsValid)
        {
            _db.CallLogs.Add(model);
            await _db.SaveChangesAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.CallTypes = await _db.CallTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        ViewBag.CallTypes = await _db.CallTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
        var item = await GetFilteredQuery()
            .Include(c => c.Updates.OrderByDescending(u => u.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (item == null)
            return NotFound();
        item.GuncellenenTarih = null;
        item.GuncellenenCagriSaat = null;
        item.GuncellemeNedeni = null;
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CallLog model, CancellationToken cancellationToken = default)
    {
        var item = await GetFilteredQuery()
            .Include(c => c.Updates)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (item == null)
            return NotFound();

        model.Tarih = model.Tarih.Date;
        var duplicate = await _db.CallLogs.AnyAsync(c =>
            c.Tarih.Date == model.Tarih && c.CagriNo == model.CagriNo && c.Id != id, cancellationToken);
        if (duplicate)
            ModelState.AddModelError(nameof(model.CagriNo), "Bu tarih ve çağrı no kombinasyonu zaten mevcut.");

        // Güncelleme ekleniyorsa: GuncellenenTarih, GuncellenenCagriSaat, GuncellemeNedeni zorunlu
        var hasUpdateData = model.GuncellenenTarih.HasValue ||
            !string.IsNullOrWhiteSpace(model.GuncellenenCagriSaat) ||
            !string.IsNullOrWhiteSpace(model.GuncellemeNedeni);
        if (hasUpdateData)
        {
            if (!model.GuncellenenTarih.HasValue)
                ModelState.AddModelError(nameof(model.GuncellenenTarih), "Güncelleme eklerken güncellenen tarih zorunludur.");
            if (string.IsNullOrWhiteSpace(model.GuncellenenCagriSaat))
                ModelState.AddModelError(nameof(model.GuncellenenCagriSaat), "Güncelleme eklerken güncellenen çağrı saati zorunludur.");
            if (string.IsNullOrWhiteSpace(model.GuncellemeNedeni))
                ModelState.AddModelError(nameof(model.GuncellemeNedeni), "Güncelleme eklerken güncelleme nedeni zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.CallTypes = await _db.CallTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            ViewBag.Updates = await _db.CallLogUpdates
                .Where(u => u.CallLogId == id)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync(cancellationToken);
            return View(model);
        }

        {
            item.SiraNo = model.SiraNo;
            item.CagriNo = model.CagriNo;
            item.CagriTuru = model.CagriTuru;
            item.TeknisyenAdi = model.TeknisyenAdi;
            item.Tarih = model.Tarih;
            item.MevcutCagriSaat = model.MevcutCagriSaat;

            if (hasUpdateData)
            {
                var update = new CallLogUpdate
                {
                    CallLogId = id,
                    GuncellenenTarih = model.GuncellenenTarih!.Value.Date,
                    GuncellenenCagriSaat = model.GuncellenenCagriSaat!.Trim(),
                    GuncellemeNedeni = model.GuncellemeNedeni!.Trim(),
                    CreatedAt = DateTime.Now
                };
                _db.CallLogUpdates.Add(update);
                item.GuncellenenTarih = update.GuncellenenTarih;
                item.GuncellenenCagriSaat = update.GuncellenenCagriSaat;
                item.GuncellemeNedeni = update.GuncellemeNedeni;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var item = await GetFilteredQuery().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (item == null)
            return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
    {
        var item = await GetFilteredQuery().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (item == null)
            return NotFound();
        _db.CallLogs.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
