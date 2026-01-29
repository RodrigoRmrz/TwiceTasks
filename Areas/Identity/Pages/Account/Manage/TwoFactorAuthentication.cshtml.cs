using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TwiceTasks.Models;

namespace TwiceTasks.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class TwoFactorAuthenticationModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TwoFactorAuthenticationModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public bool Is2faEnabled { get; set; }
    public int RecoveryCodesLeft { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    private async Task LoadAsync(ApplicationUser user)
    {
        Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
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

    public async Task<IActionResult> OnPostDisable2faAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            StatusMessage = "Error: no se pudo desactivar 2FA.";
            return RedirectToPage();
        }

        StatusMessage = "2FA desactivado.";
        return RedirectToPage();
    }
}
