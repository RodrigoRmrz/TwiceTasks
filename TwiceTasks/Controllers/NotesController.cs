using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

namespace TwiceTasks.Controllers;

/// <summary>
/// /Notes es el punto de entrada "oficial" para el apartado de notas.
/// Internamente el CRUD sigue estando en PagesController, pero esta pantalla
/// muestra el dashboard y permite filtrar por workspace sin cambiar la URL.
/// </summary>
[Authorize]
public sealed class NotesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Notes  (workspace opcional)
    public async Task<IActionResult> Index(int? workspaceId)
    {
        var userId = _userManager.GetUserId(User);

        // Lista workspaces (sidebar)
        var workspaces = await _context.Workspaces
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.Name)
            .ToListAsync();

        ViewBag.Workspaces = workspaces;

        // Archivos recientes (panel derecho)
        var recentFilesQuery = _context.FileResources
            .Where(f => f.UserId == userId);

        if (workspaceId.HasValue)
            recentFilesQuery = recentFilesQuery.Where(f => f.WorkspaceId == workspaceId.Value);

        ViewBag.RecentFiles = await recentFilesQuery
            .OrderByDescending(f => f.UploadedAt)
            .Take(6)
            .ToListAsync();

        // Sin workspace: devolvemos notas sueltas en ViewBag y un modelo vacío.
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

        // Dentro de un workspace
        var wsSelected = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId.Value);
        if (wsSelected == null || wsSelected.UserId != userId)
            return Unauthorized();

        ViewBag.Workspace = wsSelected;

        var pages = await _context.Pages
            .Include(p => p.Workspace)
            .Where(p => p.UserId == userId && p.WorkspaceId == workspaceId.Value)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        return View(pages);
    }
}
