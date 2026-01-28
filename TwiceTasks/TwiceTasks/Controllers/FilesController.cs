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
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public FilesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index(int? collectionId)
        {
            var userId = _userManager.GetUserId(User);

            var collections = await _context.Collections
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var filesQuery = _context.FileResources
                .Where(f => f.UserId == userId);

            if (collectionId.HasValue && collectionId.Value != 0)
            {
                filesQuery = filesQuery.Where(f => f.CollectionId == collectionId.Value);
            }
            else
            {
                filesQuery = filesQuery.Where(f => f.CollectionId == null);
            }

            var model = new FilesIndexViewModel
            {
                Collections = collections,
                Files = await filesQuery.ToListAsync(),
                SelectedCollectionId = collectionId ?? 0
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int? collectionId)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction(nameof(Index), new { collectionId });

            var userId = _userManager.GetUserId(User);

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", userId);
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var dbFile = new FileResource
            {
                FileName = file.FileName,
                FilePath = $"/uploads/{userId}/{file.FileName}",
                FileSize = file.Length,
                UserId = userId,
                CollectionId = (collectionId == 0 ? null : collectionId)
            };

            _context.FileResources.Add(dbFile);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { collectionId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var file = await _context.FileResources.FindAsync(id);
            if (file == null || file.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            var fullPath = Path.Combine(_env.WebRootPath, file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            _context.FileResources.Remove(file);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> UploadToPage(IFormFile file, int pageId)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction("Edit", "Pages", new { id = pageId });

            var page = await _context.Pages
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == pageId);

            if (page == null || page.Workspace!.UserId != _userManager.GetUserId(User))
                return Unauthorized();

            var userId = _userManager.GetUserId(User);

            var folder = Path.Combine(_env.WebRootPath, "uploads", userId, "pages", pageId.ToString());
            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var dbFile = new FileResource
            {
                FileName = file.FileName,
                FilePath = $"/uploads/{userId}/pages/{pageId}/{file.FileName}",
                FileSize = file.Length,
                UserId = userId,
                PageId = pageId
            };

            _context.FileResources.Add(dbFile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", "Pages", new { id = pageId });
        }
        [HttpPost]
        public async Task<IActionResult> MoveToCollection(int fileId, int? collectionId, int currentCollectionId = 0)
        {
            var userId = _userManager.GetUserId(User);

            var file = await _context.FileResources
                .Where(f => f.UserId == userId && f.Id == fileId)
                .FirstOrDefaultAsync();

            if (file == null)
                return Unauthorized();

            file.CollectionId = (collectionId == 0 ? null : collectionId);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { collectionId = currentCollectionId });
        }



    }
}
