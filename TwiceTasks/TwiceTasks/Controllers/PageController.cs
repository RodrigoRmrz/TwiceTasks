using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

namespace TwiceTasks.Controllers
{
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Page
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Pages.Include(p => p.Workspace);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Page/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        // GET: Page/Create
        public IActionResult Create()
        {
            ViewData["WorkspaceId"] = new SelectList(_context.Workspaces, "Id", "Name");
            return View();
        }

        // POST: Page/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,WorkspaceId,Title,Content,CreatedAt,UpdatedAt,IsArchived")] Page page)
        {
            if (ModelState.IsValid)
            {
                _context.Add(page);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WorkspaceId"] = new SelectList(_context.Workspaces, "Id", "Name", page.WorkspaceId);
            return View(page);
        }

        // GET: Page/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages.FindAsync(id);
            if (page == null)
            {
                return NotFound();
            }
            ViewData["WorkspaceId"] = new SelectList(_context.Workspaces, "Id", "Name", page.WorkspaceId);
            return View(page);
        }

        // POST: Page/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WorkspaceId,Title,Content,CreatedAt,UpdatedAt,IsArchived")] Page page)
        {
            if (id != page.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(page);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PageExists(page.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["WorkspaceId"] = new SelectList(_context.Workspaces, "Id", "Name", page.WorkspaceId);
            return View(page);
        }

        // GET: Page/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        // POST: Page/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page != null)
            {
                _context.Pages.Remove(page);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PageExists(int id)
        {
            return _context.Pages.Any(e => e.Id == id);
        }
    }
}
