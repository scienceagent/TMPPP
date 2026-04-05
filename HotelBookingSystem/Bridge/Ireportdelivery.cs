namespace HotelBookingSystem.Bridge
{
     /// <summary>
     /// Bridge — IMPLEMENTATION hierarchy.
     /// Defines HOW a report is delivered (the "where it goes" dimension).
     /// Completely independent from the format (the "what it looks like" dimension).
     /// Adding a new delivery (e.g. FTP, S3, Slack) requires only a new class here —
     /// zero changes to any report format class.
     /// </summary>
     public interface IReportDelivery
     {
          void Deliver(string content, string reportTitle);
     }
}