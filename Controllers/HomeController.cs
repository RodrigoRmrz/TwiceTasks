using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

namespace TwiceTasks.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Landing page para no logueados
            if (!(User?.Identity?.IsAuthenticated ?? false))
                return View();

            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var now = DateTime.Now;
                var today = DateTime.Today;

                var upcomingEvents = await _context.CalendarEvents
                    .Where(e => e.UserId == userId &&
                                (
                                    // All-day: a partir de hoy
                                    (e.AllDay && e.Date.Date >= today) ||
                                    // Timed: futuros o en curso
                                    (!e.AllDay && (e.Date >= now || (e.End != null && e.End >= now)))
                                ))
                    .OrderBy(e => e.Date)
                    .Take(6)
                    .ToListAsync();

                ViewBag.UpcomingEvents = upcomingEvents;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
