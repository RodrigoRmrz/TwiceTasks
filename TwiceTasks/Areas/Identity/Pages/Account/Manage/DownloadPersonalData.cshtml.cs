using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TwiceTasks.Models;

namespace TwiceTasks.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class DownloadPersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DownloadPersonalDataModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        var personalData = new Dictionary<string, string?>();

        // Datos marcados como personales por Identity
        var personalDataProps = typeof(ApplicationUser).GetProperties()
            .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

        foreach (var p in personalDataProps)
        {
            personalData[p.Name] = p.GetValue(user)?.ToString();
        }

        // Campos adicionales (por si no están anotados)
        personalData["DisplayName"] = user.DisplayName;
        personalData["AvatarUrl"] = user.AvatarUrl;

        var json = JsonSerializer.Serialize(personalData, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);

        Response.Headers.Append("Content-Disposition", "attachment; filename=TwiceTasks_PersonalData.json");
        return File(bytes, "application/json");
    }

    public IActionResult OnGet()
    {
        // Acceso directo por URL: vuelve a la pestaña.
        return RedirectToPage("./PersonalData");
    }
}
