using System.ComponentModel.DataAnnotations;

namespace TwiceTasks.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public ICollection<PageTag> PageTags { get; set; } = new List<PageTag>();
    }
}
