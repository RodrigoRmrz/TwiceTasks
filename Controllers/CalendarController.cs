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

    public CalendarController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var today = DateTime.Today;
        var y = year ?? today.Year;
        var m = month ?? today.Month;
        if (m < 1) m = 1;
        if (m > 12) m = 12;

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        // Próximos eventos (sidebar)
        var upcomingFrom = DateTime.Today;
        var upcoming = await _context.CalendarEvents
            .Where(e => e.UserId == userId && (e.End ?? e.Date) >= upcomingFrom)
            .OrderBy(e => e.Date)
            .Take(15)
            .ToListAsync();

        var vm = new CalendarIndexViewModel
        {
            Year = y,
            Month = m,
            UpcomingEvents = upcoming
        };

        return View(vm);
    }

    // Feed para FullCalendar (JSON)
    [HttpGet]
    public async Task<IActionResult> EventsFeed(DateTime start, DateTime end)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        // Incluimos eventos que se solapen con el rango
        var events = await _context.CalendarEvents
            .Where(e => e.UserId == userId && e.Date < end && (e.End ?? e.Date) >= start)
            .OrderBy(e => e.Date)
            .ToListAsync();

        var payload = events.Select(e => new
        {
            id = e.Id,
            title = e.Title,
            start = e.Date,
            end = e.End,
            allDay = e.AllDay
        });

        return Json(payload);
    }

    public record UpsertDto(int? Id, string Title, DateTime Start, DateTime? End, bool AllDay);
    public record MoveDto(int Id, DateTime Start, DateTime? End, bool AllDay);

    // Crear/editar (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert([FromBody] UpsertDto dto)
    {
        if (dto is null) return BadRequest();
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("El título es obligatorio.");

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        CalendarEvent entity;

        if (dto.Id.HasValue)
        {
            entity = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == dto.Id.Value && e.UserId == userId);
            if (entity is null) return NotFound();
        }
        else
        {
            entity = new CalendarEvent { UserId = userId };
            _context.CalendarEvents.Add(entity);
        }

        entity.Title = dto.Title.Trim();
        entity.AllDay = dto.AllDay;

        if (dto.AllDay)
        {
            entity.Date = dto.Start.Date;
            entity.End = dto.End?.Date; // (all-day) end es opcional. Si se usa, es fecha exclusiva (FullCalendar)
        }
        else
        {
            entity.Date = dto.Start;
            entity.End = dto.End;
        }

        await _context.SaveChangesAsync();
        return Ok(new { id = entity.Id });
    }

    // Arrastrar / resize (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move([FromBody] MoveDto dto)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var entity = await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.Id == dto.Id && e.UserId == userId);
        if (entity is null) return NotFound();

        entity.AllDay = dto.AllDay;
        if (dto.AllDay)
        {
            entity.Date = dto.Start.Date;
            entity.End = dto.End?.Date;
        }
        else
        {
            entity.Date = dto.Start;
            entity.End = dto.End;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    // Borrar (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax([FromBody] int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var entity = await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (entity is null) return NotFound();

        _context.CalendarEvents.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // Sidebar refresh
    [HttpGet]
    public async Task<IActionResult> Upcoming()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var upcomingFrom = DateTime.Today;
        var upcoming = await _context.CalendarEvents
            .Where(e => e.UserId == userId && (e.End ?? e.Date) >= upcomingFrom)
            .OrderBy(e => e.Date)
            .Take(15)
            .ToListAsync();

        var payload = upcoming.Select(e => new
        {
            id = e.Id,
            title = e.Title,
            start = e.Date,
            end = e.End,
            allDay = e.AllDay
        });

        return Json(payload);
    }

    // Fallback (forms antiguos)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, DateTime date, int? year, int? month)
    {
        if (string.IsNullOrWhiteSpace(title))
            return RedirectToAction(nameof(Index), new { year, month });

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var evt = new CalendarEvent
        {
            Title = title.Trim(),
            Date = date.Date,
            AllDay = true,
            UserId = userId
        };

        _context.CalendarEvents.Add(evt);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { year = year ?? date.Year, month = month ?? date.Month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? year, int? month)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var evt = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (evt != null)
        {
            _context.CalendarEvents.Remove(evt);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { year, month });
    }
}
