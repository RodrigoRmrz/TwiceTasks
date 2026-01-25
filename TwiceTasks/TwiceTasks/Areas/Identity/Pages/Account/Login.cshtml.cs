using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TwiceTasks.Models;

namespace TwiceTasks.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        // Ensure we always have a non-empty, local return URL
        var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
            ? Url.Content("~/")
            : returnUrl;
        if (!Url.IsLocalUrl(safeReturnUrl))
        {
            safeReturnUrl = Url.Content("~/");
        }

        ReturnUrl = safeReturnUrl;

        // Clean the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        // Ensure we always have a non-empty, local return URL
        var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
            ? Url.Content("~/")
            : returnUrl;
        if (!Url.IsLocalUrl(safeReturnUrl))
        {
            safeReturnUrl = Url.Content("~/");
        }

        ReturnUrl = safeReturnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return LocalRedirect(safeReturnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            ModelState.AddModelError(string.Empty, "La cuenta está bloqueada temporalmente.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
        return Page();
    }
}
