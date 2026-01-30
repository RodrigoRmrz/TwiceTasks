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

        // GET: Collections (Opcional, por si quieres mantener la vista de rejilla)
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

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
            ModelState.Remove("UserId");

            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;

            _context.Collections.Add(model);
            await _context.SaveChangesAsync();

            // Redirigir al gestor de archivos con la nueva colección seleccionada
            return RedirectToAction("Index", "Files", new { collectionId = model.Id });
        }


        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();
            var collection = await _context.Collections.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null) return NotFound();

            return View(collection);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Description")] Collection updated)
        {
            if (!ModelState.IsValid) return View(updated);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();
            var original = await _context.Collections.FirstOrDefaultAsync(c =>
                c.Id == updated.Id && c.UserId == userId);

            if (original == null) return Unauthorized();

            original.Name = updated.Name;
            original.Description = updated.Description;
            original.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Redirección Pro: Volver a la vista de archivos dentro de esa carpeta
            return RedirectToAction("Index", "Files", new { collectionId = original.Id });
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var collection = await _context.Collections
                .Include(c => c.Files)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null) return Unauthorized();

            return View(collection);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            var collection = await _context.Collections
                .Include(c => c.Files)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null) return Unauthorized();

            // Desasociar archivos para que no se borren (se quedan en "Todas")
            if (collection.Files != null)
            {
                foreach (var file in collection.Files)
                    file.CollectionId = null;
            }

            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();

            // Al eliminar, volvemos a la vista general de archivos
            return RedirectToAction("Index", "Files");
        }
    }
}