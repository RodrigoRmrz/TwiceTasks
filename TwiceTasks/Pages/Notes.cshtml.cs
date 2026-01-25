using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

namespace TwiceTasks.Pages;

[Authorize]
public class NotesModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotesModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IList<Workspace> Workspaces { get; private set; } = new List<Workspace>();
    public Workspace? SelectedWorkspace { get; private set; }
    public IList<Page> Pages { get; private set; } = new List<Page>();
    public IList<Page> UnassignedPages { get; private set; } = new List<Page>();
    public IList<FileResource> RecentFiles { get; private set; } = new List<FileResource>();

    [BindProperty(SupportsGet = true)]
    public int? WorkspaceId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        // Incluimos Pages para mostrar el contador sin consultas extra.
        Workspaces = await _context.Workspaces
            .Include(w => w.Pages)
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.Name)
            .ToListAsync();

        IQueryable<FileResource> recentFilesQuery = _context.FileResources
            .Where(f => f.UserId == userId);

        if (WorkspaceId.HasValue)
            recentFilesQuery = recentFilesQuery.Where(f => f.WorkspaceId == WorkspaceId.Value);

        RecentFiles = await recentFilesQuery
            .OrderByDescending(f => f.UploadedAt)
            .Take(6)
            .ToListAsync();

        if (!WorkspaceId.HasValue)
        {
            UnassignedPages = await _context.Pages
                .Include(p => p.Workspace)
                .Where(p => p.UserId == userId && p.WorkspaceId == null)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();

            Pages = new List<Page>();
            SelectedWorkspace = null;
            return Page();
        }

        SelectedWorkspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == WorkspaceId.Value && w.UserId == userId);

        if (SelectedWorkspace == null)
            return Forbid();

        Pages = await _context.Pages
            .Include(p => p.Workspace)
            .Where(p => p.UserId == userId && p.WorkspaceId == WorkspaceId.Value)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        return Page();
    }
}
