using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

namespace TwiceTasks.Controllers
{
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CollectionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Collections
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var collections = await _context.Collections
                .Where(c => c.UserId == userId)
                .Include(c => c.Files)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(collections);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Collection model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.UserId = _userManager.GetUserId(User);
            model.CreatedAt = DateTime.UtcNow;

            _context.Collections.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Files");
        }


        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var collection = await _context.Collections.FindAsync(id);

            if (collection == null)
                return NotFound();

            if (collection.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            return View(collection);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Description")] Collection updated)
        {
            if (!ModelState.IsValid)
                return View(updated);

            var userId = _userManager.GetUserId(User);
            var original = await _context.Collections.FirstOrDefaultAsync(c =>
                c.Id == updated.Id && c.UserId == userId);

            if (original == null)
                return Unauthorized();

            original.Name = updated.Name;
            original.Description = updated.Description;
            original.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            var collection = await _context.Collections
                .Include(c => c.Files)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null)
                return Unauthorized();

            return View(collection);
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var collection = await _context.Collections
                .Include(c => c.Files)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null)
                return Unauthorized();

            // Desasociar archivos en lugar de eliminarlos
            foreach (var file in collection.Files)
                file.CollectionId = null;

            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

