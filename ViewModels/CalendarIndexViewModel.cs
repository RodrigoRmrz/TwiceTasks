using TwiceTasks.Models;

namespace TwiceTasks.ViewModels;

public class CalendarIndexViewModel
{
    public int Year { get; set; }
    public int Month { get; set; } // 1-12

    // Se mantiene por compatibilidad (no es necesario para FullCalendar)
    public IReadOnlyList<CalendarEvent> Events { get; set; } = Array.Empty<CalendarEvent>();

    // Para el panel lateral (próximos eventos)
    public IReadOnlyList<CalendarEvent> UpcomingEvents { get; set; } = Array.Empty<CalendarEvent>();

    public DateTime FirstDayOfMonth => new DateTime(Year, Month, 1);
    public int DaysInMonth => DateTime.DaysInMonth(Year, Month);

    public DateTime PrevMonth => FirstDayOfMonth.AddMonths(-1);
    public DateTime NextMonth => FirstDayOfMonth.AddMonths(1);
}
