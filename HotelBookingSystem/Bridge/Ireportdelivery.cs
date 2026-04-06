using System.Threading.Tasks;

namespace HotelBookingSystem.Bridge
{
     // ── IMPLEMENTATION INTERFACE (Bridge implementor side) ────────────────────
     // Defines HOW a report is delivered / stored / sent.
     // Completely independent of WHAT the report contains.
     //
     // Changed to async Task so EmailDelivery can await real SMTP without blocking the UI.
     public interface IReportDelivery
     {
          Task DeliverAsync(string content, string reportTitle, string filename);
     }
}