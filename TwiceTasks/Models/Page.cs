using System.ComponentModel.DataAnnotations;


namespace TwiceTasks.Models
{
    public class Page
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsArchived { get; set; } = false;

        public ICollection<PageTag> PageTags { get; set; } = new List<PageTag>();

        public ICollection<FileResource>? Files { get; set; }
    }
}
