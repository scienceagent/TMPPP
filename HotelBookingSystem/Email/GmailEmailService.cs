using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using HotelBookingSystem.Config;

namespace HotelBookingSystem.Email
{
     // ── Result DTO ────────────────────────────────────────────────────────────
     public sealed class EmailResult
     {
          public bool Success { get; }
          public string Message { get; }

          private EmailResult(bool ok, string msg) { Success = ok; Message = msg; }
          public static EmailResult Ok(string msg = "Sent") => new(true, msg);
          public static EmailResult Fail(string msg) => new(false, msg);
     }

     // ── Email message ─────────────────────────────────────────────────────────
     public sealed class EmailMessage
     {
          public string To { get; set; } = string.Empty;
          public string Subject { get; set; } = string.Empty;
          public string Body { get; set; } = string.Empty;
          public bool IsHtml { get; set; }
          public string? AttachmentPath { get; set; }
          public string? AttachmentName { get; set; }
          public string? AttachmentContent { get; set; }  // inline text content (no file needed)
          public string? AttachmentMimeType { get; set; }
     }

     // ── Real Gmail SMTP sender ────────────────────────────────────────────────
     public sealed class GmailEmailService
     {
          private readonly GmailConfig _config;

          public GmailEmailService() : this(AppSettings.Instance.GmailDefaults) { }

          public GmailEmailService(GmailConfig config)
          {
               _config = config;
          }

          public async Task<EmailResult> SendAsync(EmailMessage message)
          {
               if (string.IsNullOrWhiteSpace(_config.Email) ||
                   string.IsNullOrWhiteSpace(_config.AppPassword))
                    return EmailResult.Fail("Gmail credentials not configured in appsettings.json.");

               if (string.IsNullOrWhiteSpace(message.To))
                    return EmailResult.Fail("Recipient email address is required.");

               try
               {
                    using var mail = new MailMessage();
                    mail.From = new MailAddress(_config.Email, _config.DisplayName);
                    mail.To.Add(message.To);
                    mail.Subject = message.Subject;
                    mail.Body = message.Body;
                    mail.IsBodyHtml = message.IsHtml;
                    mail.BodyEncoding = Encoding.UTF8;

                    // ── Inline text attachment (report content) ───────────────────
                    if (!string.IsNullOrEmpty(message.AttachmentContent) &&
                        !string.IsNullOrEmpty(message.AttachmentName))
                    {
                         byte[] bytes = Encoding.UTF8.GetBytes(message.AttachmentContent);
                         var stream = new System.IO.MemoryStream(bytes);
                         string mime = message.AttachmentMimeType ?? "text/plain";
                         mail.Attachments.Add(new Attachment(stream, message.AttachmentName, mime));
                    }
                    // ── File attachment ────────────────────────────────────────────
                    else if (!string.IsNullOrEmpty(message.AttachmentPath) &&
                             System.IO.File.Exists(message.AttachmentPath))
                    {
                         mail.Attachments.Add(new Attachment(message.AttachmentPath));
                    }

                    using var smtp = new SmtpClient("smtp.gmail.com", 587)
                    {
                         EnableSsl = true,
                         UseDefaultCredentials = false,
                         Credentials = new NetworkCredential(_config.Email, _config.AppPassword),
                         DeliveryMethod = SmtpDeliveryMethod.Network,
                         Timeout = 15_000   // 15 s
                    };

                    await smtp.SendMailAsync(mail);
                    return EmailResult.Ok($"Email sent to {message.To}");
               }
               catch (SmtpException ex)
               {
                    return EmailResult.Fail($"SMTP error: {ex.StatusCode} — {ex.Message}");
               }
               catch (Exception ex)
               {
                    return EmailResult.Fail($"Send failed: {ex.Message}");
               }
          }

          // ── Convenience overload ───────────────────────────────────────────────
          public async Task<EmailResult> SendPlainAsync(string to, string subject, string body)
              => await SendAsync(new EmailMessage
              {
                   To = to,
                   Subject = subject,
                   Body = body,
                   IsHtml = false
              });
     }
}