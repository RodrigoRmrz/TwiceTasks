using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

    namespace TwiceTasks.Models
    {
        public class Collection
        {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(300)]
        public string? Description { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        public ICollection<FileResource>? Files { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    }