using Microsoft.AspNetCore.Identity;
namespace TwiceTasks.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Estos son Campos adicionales
        public string? DisplayName { get; set; }
        public string? AvatarUrl { get; set; }

        // Relación con Workspaces
        public ICollection<Workspace>? Workspaces { get; set; }
    }
}