using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;
using TwiceTasks.ViewModels;

namespace TwiceTasks.Controllers
{
    [Authorize]
    public class DesarrolloController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DesarrolloController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var snippets = await _context.CodeSnippets
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();

            return View(new DesarrolloIndexViewModel
            {
                Snippets = snippets
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(DesarrolloIndexViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            if (!ModelState.IsValid)
            {
                model.Snippets = await _context.CodeSnippets
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.UpdatedAt)
                    .ToListAsync();

                return View("Index", model);
            }

            var now = DateTime.UtcNow;
            var title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim();
            var content = (model.Content ?? string.Empty).TrimEnd();

            if (model.Id is null or 0)
            {
                var snippet = new CodeSnippet
                {
                    UserId = userId,
                    Title = title,
                    Content = content,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.CodeSnippets.Add(snippet);
            }
            else
            {
                var snippet = await _context.CodeSnippets
                    .FirstOrDefaultAsync(s => s.Id == model.Id && s.UserId == userId);

                if (snippet == null) return NotFound();

                snippet.Title = title;
                snippet.Content = content;
                snippet.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var snippet = await _context.CodeSnippets
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (snippet == null) return NotFound();

            _context.CodeSnippets.Remove(snippet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
