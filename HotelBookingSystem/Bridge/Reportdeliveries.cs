using System.Collections.Generic;

namespace HotelBookingSystem.Bridge
{
     /// <summary>
     /// Delivery 1 — appends report to the in-app activity log.
     /// </summary>
     public class LogDelivery : IReportDelivery
     {
          private readonly List<string> _log;

          public LogDelivery(List<string> log)
          {
               _log = log;
          }

          public void Deliver(string content, string reportTitle)
          {
               _log.Add($"[Bridge:Delivery:Log] '{reportTitle}' written to activity log ({content.Length} chars).");
               _log.Add(content);
          }
     }

     /// <summary>
     /// Delivery 2 — simulates saving to a local file path.
     /// </summary>
     public class FileDelivery : IReportDelivery
     {
          private readonly string _basePath;
          private readonly List<string> _log;

          public FileDelivery(string basePath, List<string> log)
          {
               _basePath = basePath;
               _log = log;
          }

          public void Deliver(string content, string reportTitle)
          {
               var filename = $"{reportTitle.Replace(" ", "_")}_{System.DateTime.Now:yyyyMMdd_HHmmss}";
               _log.Add($"[Bridge:Delivery:File] Saved '{filename}' to {_basePath} ({content.Length} bytes).");
          }
     }

     /// <summary>
     /// Delivery 3 — simulates sending the report via email.
     /// </summary>
     public class EmailDelivery : IReportDelivery
     {
          private readonly string _recipient;
          private readonly List<string> _log;

          public EmailDelivery(string recipient, List<string> log)
          {
               _recipient = recipient;
               _log = log;
          }

          public void Deliver(string content, string reportTitle)
          {
               _log.Add($"[Bridge:Delivery:Email] → {_recipient}: Subject='{reportTitle}' ({content.Length} chars attached).");
          }
     }
}