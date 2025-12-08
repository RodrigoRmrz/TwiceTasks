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

    public CalendarController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var events = await _context.CalendarEvents
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Date)
            .ToListAsync();

        return View(events);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string title, DateTime date)
    {
        var evt = new CalendarEvent
        {
            Title = title,
            Date = date,
            UserId = _userManager.GetUserId(User)
        };

        _context.CalendarEvents.Add(evt);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
