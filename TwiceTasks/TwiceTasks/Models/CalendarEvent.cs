using System.ComponentModel.DataAnnotations;

namespace TwiceTasks.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        // Usuario propietario
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Opcional: vincular a una página
        public int? PageId { get; set; }
        public Page? Page { get; set; }
        public DateTime? End { get; set; }
        public bool AllDay { get; set; } = true;

    }
}
