using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

namespace TwiceTasks.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string q)
        {
            var userId = _userManager.GetUserId(User);

            var pages = _context.Pages
                .Include(p => p.Workspace)
                .Include(p => p.PageTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.Workspace!.UserId == userId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                pages = pages.Where(p =>
                    p.Title.Contains(q) ||
                    p.PageTags.Any(pt => pt.Tag.Name.Contains(q)));
            }

            return View(await pages.ToListAsync());
        }
    }
}
