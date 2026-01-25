using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwiceTasks.Models
{
    public class Workspace
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        public ICollection<Page>? Pages { get; set; }
        public ICollection<FileResource>? Files { get; set; }

        public DateTime? UpdatedAt { get; set; } = null;
    }

}
