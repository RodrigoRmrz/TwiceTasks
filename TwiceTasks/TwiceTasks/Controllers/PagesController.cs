using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

namespace TwiceTasks.Controllers
{
    [Authorize]
    public class PagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // LISTAR páginas de un workspace
        public async Task<IActionResult> Index(int workspaceId)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Pages)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null || workspace.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            ViewBag.Workspace = workspace;
            return View(workspace.Pages);
        }

        // CREAR página
        public IActionResult Create(int workspaceId)
        {
            ViewBag.WorkspaceId = workspaceId;
            return View(new Page { WorkspaceId = workspaceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Page page)
        {
            var workspace = await _context.Workspaces.FindAsync(page.WorkspaceId);
            if (workspace == null || workspace.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(page);

            _context.Pages.Add(page);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { workspaceId = page.WorkspaceId });
        }

        // EDITAR página
        public async Task<IActionResult> Edit(int id)
        {
            var page = await _context.Pages
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null || page.Workspace!.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Page page)
        {
            var existing = await _context.Pages
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == page.Id);

            if (existing == null || existing.Workspace!.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(page);

            existing.Title = page.Title;
            existing.Content = page.Content;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { workspaceId = existing.WorkspaceId });
        }
        // GET: Pages/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var page = await _context.Pages
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null || page.Workspace!.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            return View(page);
        }

        // POST: Pages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = await _context.Pages
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null || page.Workspace!.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            var workspaceId = page.WorkspaceId;

            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { workspaceId });
        }

    }
}
