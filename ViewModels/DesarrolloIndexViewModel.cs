using System.ComponentModel.DataAnnotations;
using TwiceTasks.Models;

namespace TwiceTasks.ViewModels
{
    public class DesarrolloIndexViewModel
    {
        public int? Id { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public List<CodeSnippet> Snippets { get; set; } = new();
    }
}
