using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TwiceTasks.Models;

namespace TwiceTasks.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class EmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public EmailModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string Email { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Nuevo email")]
        public string NewEmail { get; set; } = string.Empty;
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        Email = await _userManager.GetEmailAsync(user) ?? string.Empty;
        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        Input = new InputModel { NewEmail = Email };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var currentEmail = await _userManager.GetEmailAsync(user);
        if (string.Equals(Input.NewEmail, currentEmail, StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = "No se han realizado cambios.";
            return RedirectToPage();
        }

        // En esta app no forzamos confirmación; cambiamos directamente.
        var setEmailResult = await _userManager.SetEmailAsync(user, Input.NewEmail);
        if (!setEmailResult.Succeeded)
        {
            StatusMessage = "Error: no se pudo cambiar el email.";
            return RedirectToPage();
        }

        // A menudo el username coincide con el email
        var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.NewEmail);
        if (!setUserNameResult.Succeeded)
        {
            StatusMessage = "Error: no se pudo actualizar el nombre de usuario.";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Email actualizado.";
        return RedirectToPage();
    }
}
