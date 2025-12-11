using System.ComponentModel.DataAnnotations;

namespace TwiceTasks.Models
{
    public class FileResource
    {
        [Key]
        public int Id { get; set; }

        // Archivo
        [Required, MaxLength(255)]
        public string FileName { get; set; } = "";

        [Required]
        public string FilePath { get; set; } = "";

        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Usuario dueño del archivo
        [Required]
        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        // Workspace opcional
        public int? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        // Page opcional
        public int? PageId { get; set; }
        public Page? Page { get; set; }

        // ⭐ COLECCIÓN (LO QUE ESTAMOS CREANDO)
        public int? CollectionId { get; set; }   // ✔ Ahora sí es un int nullable
        public Collection? Collection { get; set; }
    }
}
