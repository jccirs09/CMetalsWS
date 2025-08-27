using Heron.MudCalendar;
using MudBlazor;
using CMetalsWS.Data;

namespace CMetalsWS.Data.Scheduling
{
    // Derive from CalendarItem so we can carry our own key and extra fields
    public sealed class WorkOrderCalendarItem : CalendarItem
    {
        // Your key you can set/read freely
        public int WorkOrderId { get; set; }

        // Optional context fields used by templates/handlers
        public string? WorkOrderNumber { get; set; }
        public string? TagNumber { get; set; }
        public MachineCategory MachineCategory { get; set; }
        public string? LocationCode { get; set; }
        public Color ChipColor { get; set; } = Color.Primary;
    }
}
