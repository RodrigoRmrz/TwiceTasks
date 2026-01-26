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
            // Si NO está logueado -> mostramos landing pública (tu vista ya lo maneja con IsAuthenticated)
            if (!(User?.Identity?.IsAuthenticated ?? false))
                return View();

            // Si está logueado, cargamos próximos eventos
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return View();

            var now = DateTime.Now;

            var upcomingEvents = await _context.CalendarEvents
                .Where(e => e.UserId == userId &&
                            (e.Date >= now || (e.End != null && e.End >= now)))
                .OrderBy(e => e.Date)
                .Take(5)
                .ToListAsync();

            ViewBag.UpcomingEvents = upcomingEvents;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
