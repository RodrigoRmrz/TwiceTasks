using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TwiceTasks.Models;

namespace TwiceTasks.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class DeletePersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<DeletePersonalDataModel> _logger;

    public DeletePersonalDataModel(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<DeletePersonalDataModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public bool RequirePassword { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        if (RequirePassword)
        {
            if (string.IsNullOrWhiteSpace(Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Debes introducir tu contraseña.");
                return Page();
            }

            var passwordOk = await _userManager.CheckPasswordAsync(user, Input.Password);
            if (!passwordOk)
            {
                ModelState.AddModelError(string.Empty, "La contraseña no es correcta.");
                return Page();
            }
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            StatusMessage = "Error: no se pudo eliminar la cuenta.";
            return RedirectToPage();
        }

        await _signInManager.SignOutAsync();
        _logger.LogInformation("Usuario eliminado.");

        return RedirectToPage("/Index", new { area = "" });
    }
}
