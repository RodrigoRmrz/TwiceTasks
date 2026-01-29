using TwiceTasks.Data;
using TwiceTasks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TwiceTasks.Controllers
{
    [Authorize]
    public class WorkspacesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkspacesController(ApplicationDbContext context,
                                    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Workspaces
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var workspaces = await _context.Workspaces
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(workspaces);
        }

        // GET: Workspaces/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Workspaces/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Workspace workspace)
        {
            var userId = _userManager.GetUserId(User);

            workspace.UserId = userId;
            workspace.CreatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.Add(workspace);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workspace);
        }

        // GET: Workspaces/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var workspace = await _context.Workspaces.FindAsync(id);
            if (workspace == null) return NotFound();

            // Seguridad: solo el dueño puede editar
            if (!IsOwner(workspace)) return Unauthorized();

            return View(workspace);
        }

        // POST: Workspaces/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Workspace workspace)
        {
            if (id != workspace.Id) return NotFound();

            if (!IsOwner(workspace)) return Unauthorized();

            if (ModelState.IsValid)
            {
                try
                {
                    workspace.UpdatedAt = DateTime.UtcNow;
                    _context.Update(workspace);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkspaceExists(workspace.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(workspace);
        }

        // GET: Workspaces/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(m => m.Id == id);

            if (workspace == null) return NotFound();
            if (!IsOwner(workspace)) return Unauthorized();

            return View(workspace);
        }

        // POST: Workspaces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workspace = await _context.Workspaces.FindAsync(id);

            if (workspace == null) return NotFound();
            if (!IsOwner(workspace)) return Unauthorized();

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------

        private bool WorkspaceExists(int id)
        {
            return _context.Workspaces.Any(e => e.Id == id);
        }

        private bool IsOwner(Workspace ws)
        {
            var userId = _userManager.GetUserId(User);
            return ws.UserId == userId;
        }
    }
}
