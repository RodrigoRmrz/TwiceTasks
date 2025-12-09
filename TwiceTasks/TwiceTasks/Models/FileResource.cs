using System.ComponentModel.DataAnnotations;

namespace TwiceTasks.Models
{
    public class FileResource
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } = "";

        [Required]
        public string FilePath { get; set; } = "";

        public long FileSize { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        public ApplicationUser? User { get; set; }

        public int? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        public int? PageId { get; set; }
        public Page? Page { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
