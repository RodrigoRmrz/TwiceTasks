using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;
using TwiceTasks.ViewModels;

[Authorize]
public class CalendarController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CalendarController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var now = DateTime.Today;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        // clamp just in case
        if (m < 1) m = 1;
        if (m > 12) m = 12;

        var userId = _userManager.GetUserId(User);
        var start = new DateTime(y, m, 1);
        var end = start.AddMonths(1);

        // Para el calendario (FullCalendar) cargaremos por AJAX, pero mantenemos estos datos
        // por si quieres usarlos en UI/server-side.
        var events = await _context.CalendarEvents
            .Where(e => e.UserId == userId && e.Date >= start && e.Date < end)
            .OrderBy(e => e.Date)
            .ToListAsync();

        // Próximos eventos (no limitado al mes actual)
        var upcoming = await _context.CalendarEvents
            .Where(e => e.UserId == userId && e.Date.Date >= now)
            .OrderBy(e => e.Date)
            .Take(15)
            .ToListAsync();

        var vm = new CalendarIndexViewModel
        {
            Year = y,
            Month = m,
            Events = events,
            UpcomingEvents = upcoming
        };

        return View(vm);
    }

    /// <summary>
    /// Feed JSON para FullCalendar (rango visible).
    /// FullCalendar envía start/end en querystring.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EventsFeed(DateTime start, DateTime end)
    {
        var userId = _userManager.GetUserId(User);

        // Traemos eventos que intersecten el rango.
        // (Si End es null, asumimos que dura al menos hasta Date.)
        var evts = await _context.CalendarEvents
            .Where(e => e.UserId == userId)
            .Where(e => e.Date < end && (e.End ?? e.Date) >= start)
            .OrderBy(e => e.Date)
            .ToListAsync();

        var data = evts.Select(e => new
        {
            id = e.Id,
            title = e.Title,
            start = e.Date,
            end = e.AllDay
                ? (e.End ?? e.Date.Date.AddDays(1))
                : (e.End ?? e.Date.AddHours(1)),
            allDay = e.AllDay
        });

        return Json(data);
    }

    /// <summary>
    /// Próximos eventos (para refrescar sidebar sin recargar página).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Upcoming(int take = 15)
    {
        var userId = _userManager.GetUserId(User);
        var now = DateTime.Now;

        var upcoming = await _context.CalendarEvents
            .Where(e => e.UserId == userId && (e.End ?? e.Date) >= now)
            .OrderBy(e => e.Date)
            .Take(Math.Clamp(take, 1, 50))
            .Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.Date,
                end = e.End,
                allDay = e.AllDay
            })
            .ToListAsync();

        return Json(upcoming);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert([FromBody] CalendarUpsertDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { error = "Título requerido" });

        var userId = _userManager.GetUserId(User);

        if (dto.Id == null || dto.Id <= 0)
        {
            var evt = new CalendarEvent
            {
                Title = dto.Title.Trim(),
                Date = dto.AllDay ? dto.Start.Date : dto.Start,
                End = dto.End,
                AllDay = dto.AllDay,
                UserId = userId
            };

            _context.CalendarEvents.Add(evt);
            await _context.SaveChangesAsync();
            return Ok(new { id = evt.Id });
        }
        else
        {
            var evt = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == dto.Id && e.UserId == userId);
            if (evt == null) return NotFound();

            evt.Title = dto.Title.Trim();
            evt.AllDay = dto.AllDay;
            evt.Date = dto.AllDay ? dto.Start.Date : dto.Start;
            evt.End = dto.End;

            await _context.SaveChangesAsync();
            return Ok(new { id = evt.Id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move([FromBody] CalendarMoveDto dto)
    {
        var userId = _userManager.GetUserId(User);
        var evt = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == dto.Id && e.UserId == userId);
        if (evt == null) return NotFound();

        evt.AllDay = dto.AllDay;
        evt.Date = dto.AllDay ? dto.Start.Date : dto.Start;
        evt.End = dto.End;

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax([FromBody] int id)
    {
        var userId = _userManager.GetUserId(User);
        var evt = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (evt == null) return NotFound();

        _context.CalendarEvents.Remove(evt);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, DateTime date, int? year, int? month)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            // back to calendar
            return RedirectToAction(nameof(Index), new { year, month });
        }

        var evt = new CalendarEvent
        {
            Title = title,
            Date = date,
            UserId = _userManager.GetUserId(User)
        };

        _context.CalendarEvents.Add(evt);
        await _context.SaveChangesAsync();

        // keep current month view
        return RedirectToAction(nameof(Index), new { year = year ?? date.Year, month = month ?? date.Month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? year, int? month)
    {
        var userId = _userManager.GetUserId(User);
        var evt = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (evt != null)
        {
            _context.CalendarEvents.Remove(evt);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { year, month });
    }
}
