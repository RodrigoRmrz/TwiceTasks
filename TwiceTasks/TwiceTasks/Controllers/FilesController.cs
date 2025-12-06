using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Data;
using TwiceTasks.Models;

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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var files = await _context.FileResources
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();

            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return RedirectToAction(nameof(Index));

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
                UserId = userId
            };

            _context.FileResources.Add(dbFile);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
    }
}
