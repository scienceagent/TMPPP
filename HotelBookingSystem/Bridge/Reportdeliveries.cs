using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HotelBookingSystem.Config;
using HotelBookingSystem.Email;

namespace HotelBookingSystem.Bridge
{
     // ── DELIVERY 1: Log Delivery ──────────────────────────────────────────────
     // Appends the report to the in-app activity log.
     // No file I/O or network — always works for demo.
     public class LogDelivery : IReportDelivery
     {
          private readonly List<string> _log;

          public LogDelivery(List<string> log) => _log = log;

          public Task DeliverAsync(string content, string reportTitle, string filename)
          {
               _log.Add($"[Bridge:Log] '{reportTitle}' → activity log ({content.Length} chars).");
               _log.Add(content);
               return Task.CompletedTask;
          }
     }

     // ── DELIVERY 2: File Delivery ─────────────────────────────────────────────
     // Writes the report to C:\Users\grigo\Desktop\rapoarte (from appsettings.json).
     // Creates the directory if it does not exist.
     public class FileDelivery : IReportDelivery
     {
          private readonly string _outputDirectory;
          private readonly List<string> _log;

          // Parameterless constructor reads from appsettings.json
          public FileDelivery(List<string> log)
          {
               _outputDirectory = AppSettings.Instance.ReportSettings.OutputDirectory;
               _log = log;
          }

          public FileDelivery(string directory, List<string> log)
          {
               _outputDirectory = directory;
               _log = log;
          }

          public Task DeliverAsync(string content, string reportTitle, string filename)
          {
               try
               {
                    Directory.CreateDirectory(_outputDirectory);
                    string fullPath = Path.Combine(_outputDirectory, filename);
                    File.WriteAllText(fullPath, $"=== {reportTitle} ===\n{content}", Encoding.UTF8);
                    _log.Add($"[Bridge:File] Saved '{filename}' → {fullPath}");
                    return Task.CompletedTask;
               }
               catch (Exception ex)
               {
                    _log.Add($"[Bridge:File] ERROR saving '{filename}': {ex.Message}");
                    return Task.CompletedTask;
               }
          }
     }

     // ── DELIVERY 3: Email Delivery ────────────────────────────────────────────
     // Sends the report as an email attachment via real Gmail SMTP.
     // Reads credentials from appsettings.json.
     // The report content is attached as a text file — no temp file needed.
     public class EmailDelivery : IReportDelivery
     {
          private readonly string _recipient;
          private readonly List<string> _log;
          private readonly GmailEmailService _gmail;

          // Production constructor — reads Gmail config from appsettings.json
          public EmailDelivery(string recipient, List<string> log)
          {
               _recipient = recipient;
               _log = log;
               _gmail = new GmailEmailService();
          }

          public async Task DeliverAsync(string content, string reportTitle, string filename)
          {
               _log.Add($"[Bridge:Email] Sending '{reportTitle}' → {_recipient} ...");

               string mimeType = filename.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                   ? "text/html"
                   : filename.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                       ? "text/csv"
                       : "text/plain";

               string bodyHtml = $@"
<html><body>
<p>Dear Manager,</p>
<p>Please find attached the <strong>{reportTitle}</strong> generated on {DateTime.Now:dd MMM yyyy HH:mm}.</p>
<p>This report was generated automatically by the <em>Grand Horizon Hotel Property Management System</em>.</p>
<br/>
<p>Best regards,<br/>Grand Horizon Hotel PMS</p>
</body></html>";

               var message = new EmailMessage
               {
                    To = _recipient,
                    Subject = $"[Hotel PMS] {reportTitle} — {DateTime.Now:dd MMM yyyy}",
                    Body = bodyHtml,
                    IsHtml = true,
                    AttachmentContent = content,
                    AttachmentName = filename,
                    AttachmentMimeType = mimeType
               };

               var result = await _gmail.SendAsync(message);

               _log.Add(result.Success
                   ? $"[Bridge:Email] ✓ Sent to {_recipient}"
                   : $"[Bridge:Email] ✗ Failed: {result.Message}");
          }
     }

     // ── DELIVERY 4: File + Email combined ────────────────────────────────────
     // Saves locally AND sends by email — common in real hotel PMS workflows.
     public class FileAndEmailDelivery : IReportDelivery
     {
          private readonly FileDelivery _file;
          private readonly EmailDelivery _email;

          public FileAndEmailDelivery(string recipient, List<string> log)
          {
               _file = new FileDelivery(log);
               _email = new EmailDelivery(recipient, log);
          }

          public async Task DeliverAsync(string content, string reportTitle, string filename)
          {
               await _file.DeliverAsync(content, reportTitle, filename);
               await _email.DeliverAsync(content, reportTitle, filename);
          }
     }
}