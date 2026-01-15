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

        private async Task<List<Workspace>> GetUserWorkspacesAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Workspaces
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        // LISTAR (todas o por workspace)
        public async Task<IActionResult> Index(int? workspaceId)
        {
            var userId = _userManager.GetUserId(User);

            // Lista workspaces (para offcanvas y dropdown mover)
            var workspaces = await _context.Workspaces
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.Name)
                .ToListAsync();

            ViewBag.Workspaces = workspaces;

            // Home (sin workspace): mostramos NOTAS SUELTAS + "recientes" en la vista
            if (!workspaceId.HasValue)
            {
                ViewBag.Workspace = null;

                var unassigned = await _context.Pages
                    .Include(p => p.Workspace)
                    .Where(p => p.UserId == userId && p.WorkspaceId == null)
                    .OrderByDescending(p => p.UpdatedAt)
                    .ToListAsync();

                ViewBag.UnassignedPages = unassigned;

                return View(Enumerable.Empty<Page>());
            }

            // Dentro de workspace
            var wsSelected = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId.Value);
            if (wsSelected == null || wsSelected.UserId != userId) return Unauthorized();

            ViewBag.Workspace = wsSelected;

            var pages = await _context.Pages
                .Include(p => p.Workspace)
                .Where(p => p.UserId == userId && p.WorkspaceId == workspaceId.Value)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();

            return View(pages);
        }


        // CREATE GET (workspace opcional)
        public async Task<IActionResult> Create(int? workspaceId)
        {
            var userId = _userManager.GetUserId(User);

            ViewBag.Workspaces = await GetUserWorkspacesAsync();

            // si viene workspaceId, validar que es del usuario
            if (workspaceId.HasValue)
            {
                var ws = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId.Value);
                if (ws == null || ws.UserId != userId)
                    return Unauthorized();
            }

            return View(new Page { WorkspaceId = workspaceId });
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Page page)
        {
            var userId = _userManager.GetUserId(User);

            // Validar workspace solo si viene
            if (page.WorkspaceId.HasValue)
            {
                var ws = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == page.WorkspaceId.Value);
                if (ws == null || ws.UserId != userId)
                    return Unauthorized();
            }

            page.UserId = userId!;
            page.CreatedAt = DateTime.UtcNow;
            page.UpdatedAt = DateTime.UtcNow;

            _context.Pages.Add(page);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { workspaceId = page.WorkspaceId });
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            ViewBag.Workspaces = await GetUserWorkspacesAsync();

            var page = await _context.Pages
                .Include(p => p.Workspace)
                .Include(p => p.PageTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null || page.UserId != userId)
                return Unauthorized();

            ViewBag.Files = await _context.FileResources
                .Where(f => f.PageId == id)
                .ToListAsync();

            ViewBag.Tags = page.PageTags.Select(pt => pt.Tag).ToList();

            return View(page);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Page page)
        {
            var userId = _userManager.GetUserId(User);

            var existing = await _context.Pages.FirstOrDefaultAsync(p => p.Id == page.Id);
            if (existing == null || existing.UserId != userId)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                ViewBag.Workspaces = await GetUserWorkspacesAsync();
                return View(page);
            }

            // Validar workspace destino (si viene)
            if (page.WorkspaceId.HasValue)
            {
                var ws = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == page.WorkspaceId.Value);
                if (ws == null || ws.UserId != userId)
                    return Unauthorized();
            }

            existing.Title = page.Title;
            existing.Content = page.Content;
            existing.WorkspaceId = page.WorkspaceId; // ✅ permitir mover desde Edit
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { workspaceId = existing.WorkspaceId });
        }

        // ✅ MOVER (desde Index)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Move(int pageId, int? workspaceId, int? returnWorkspaceId)
        {
            var userId = _userManager.GetUserId(User);

            var page = await _context.Pages.FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null || page.UserId != userId)
                return Unauthorized();

            // workspaceId null => "Sin workspace"
            if (workspaceId.HasValue)
            {
                var ws = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId.Value);
                if (ws == null || ws.UserId != userId)
                    return Unauthorized();
            }

            page.WorkspaceId = workspaceId;
            page.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { workspaceId = returnWorkspaceId });
        }

        [HttpPost]
        public async Task<IActionResult> AddTag(int pageId, string tagName)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(tagName))
                return RedirectToAction(nameof(Edit), new { id = pageId });

            tagName = tagName.Trim().ToLower();

            var page = await _context.Pages
                .Include(p => p.PageTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == pageId);

            if (page == null || page.UserId != userId)
                return Unauthorized();

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);

            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            if (!page.PageTags.Any(pt => pt.TagId == tag.Id))
            {
                page.PageTags.Add(new PageTag { PageId = pageId, TagId = tag.Id });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = pageId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTag(int pageId, int tagId)
        {
            var userId = _userManager.GetUserId(User);

            var pageTag = await _context.PageTags
                .Include(pt => pt.Page)
                .FirstOrDefaultAsync(pt => pt.PageId == pageId && pt.TagId == tagId);

            if (pageTag == null || pageTag.Page.UserId != userId)
                return Unauthorized();

            _context.PageTags.Remove(pageTag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = pageId });
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            var page = await _context.Pages
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null || page.UserId != userId)
                return Unauthorized();

            return View(page);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var page = await _context.Pages.FirstOrDefaultAsync(p => p.Id == id);
            if (page == null || page.UserId != userId)
                return Unauthorized();

            var workspaceId = page.WorkspaceId;

            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { workspaceId });
        }
    }
}
