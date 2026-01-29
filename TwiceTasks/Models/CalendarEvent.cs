using System.ComponentModel.DataAnnotations;

namespace TwiceTasks.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        // Inicio del evento (se mantiene el nombre "Date" por compatibilidad con la BD)
        [Required]
        public DateTime Date { get; set; }

        // Fin opcional (para eventos con hora o multi-día)
        public DateTime? End { get; set; }

        // True si es evento de día completo
        public bool AllDay { get; set; } = true;

        // Usuario propietario
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // Opcional: vincular a una página
        public int? PageId { get; set; }
        public Page? Page { get; set; }
    }
}
