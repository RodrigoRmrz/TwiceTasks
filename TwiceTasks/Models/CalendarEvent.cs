using System.ComponentModel.DataAnnotations;

namespace TwiceTasks.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Inicio del evento. (Se llamaba "Date" originalmente; se mantiene para compatibilidad)
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Fin del evento (opcional). Si es null, el cliente asumirá una duración por defecto.
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        /// Evento de día completo.
        /// </summary>
        public bool AllDay { get; set; } = true;

        // Usuario propietario
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        // Opcional: vincular a una página
        public int? PageId { get; set; }
        public Page? Page { get; set; }
    }
}
