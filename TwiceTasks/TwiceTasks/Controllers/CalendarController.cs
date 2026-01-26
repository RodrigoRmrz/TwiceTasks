using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

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

    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> EventsFeed(DateTime start, DateTime end)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

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
            entity.End = dto.End?.Date; // End exclusivo si viene de FullCalendar (OK)
        }
        else
        {
            entity.Date = dto.Start;
            entity.End = dto.End;
        }

        await _context.SaveChangesAsync();
        return Ok(new { id = entity.Id });
    }

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
}
