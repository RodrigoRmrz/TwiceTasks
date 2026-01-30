using System.ComponentModel.DataAnnotations;

namespace TwiceTasks.Models
{
    public class CodeSnippet
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        public ApplicationUser? User { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
