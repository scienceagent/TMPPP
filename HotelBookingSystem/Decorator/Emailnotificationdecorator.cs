using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using HotelBookingSystem.Email;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.Decorator
{
     // ── DECORATOR 2: Real Email Notification ──────────────────────────────────
          // Sends a real email to the guest's email address via Gmail SMTP
     // every time a booking event fires through the decorator chain.
     //
     public class EmailNotificationDecorator : BookingNotificationDecorator
     {
          private readonly List<string> _log;
          private readonly GmailEmailService _gmail;
          private readonly IUserRepository _userRepository;
          private static readonly CultureInfo USD = CultureInfo.GetCultureInfo("en-US");

          public EmailNotificationDecorator(IBookingNotificationService inner,
                                            List<string> log,
                                            IUserRepository userRepository)
              : base(inner)
          {
               _log = log;
               _gmail = new GmailEmailService();
               _userRepository = userRepository;
          }

          // ── BOOKING CREATED ────────────────────────────────────────────────────
          public override void NotifyBookingCreated(Booking booking)
          {
               _log.Add($"[Decorator:Email] Sending booking-created email for {booking.BookingId[..8]}...");
               _ = SendBookingCreatedAsync(booking); // fire-and-forget — doesn't block UI
               _inner.NotifyBookingCreated(booking);
          }

          // ── BOOKING CONFIRMED ──────────────────────────────────────────────────
          public override void NotifyBookingConfirmed(Booking booking)
          {
               _log.Add($"[Decorator:Email] Sending booking-confirmed email for {booking.BookingId[..8]}...");
               _ = SendBookingConfirmedAsync(booking);
               _inner.NotifyBookingConfirmed(booking);
          }

          // ── BOOKING CANCELLED ──────────────────────────────────────────────────
          public override void NotifyBookingCancelled(Booking booking)
          {
               _log.Add($"[Decorator:Email] Sending booking-cancelled email for {booking.BookingId[..8]}...");
               _ = SendBookingCancelledAsync(booking);
               _inner.NotifyBookingCancelled(booking);
          }

          // ── Private senders ────────────────────────────────────────────────────

          private string GetGuestEmail(string userId)
          {
               var guest = _userRepository.FindById(userId) as Guest;
               return guest?.Email ?? string.Empty;
          }

          private async Task SendBookingCreatedAsync(Booking booking)
          {
               string emailAddress = GetGuestEmail(booking.UserId);
               if (string.IsNullOrEmpty(emailAddress))
               {
                    _log.Add($"[Decorator:Email] ✗ Failed: No email found for guest {booking.UserId}");
                    return;
               }

               int nights = (booking.CheckOutDate - booking.CheckInDate).Days;
               var result = await _gmail.SendAsync(new EmailMessage
               {
                    To = emailAddress,
                    Subject = $"[Grand Horizon] New Booking Created — #{booking.BookingId[..8]}",
                    IsHtml = true,
                    Body = BuildHtml(
                       title: "New Booking Created",
                       colour: "#1D4ED8",
                       icon: "📋",
                       lines: new[]
                       {
                        $"Booking ID   : <strong>{booking.BookingId[..8]}…</strong>",
                        $"Type         : <strong>{booking.BookingType}</strong>",
                        $"Status       : <strong style='color:#D97706'>Pending</strong>",
                        $"Check-In     : <strong>{booking.CheckInDate:dd MMM yyyy}</strong>",
                        $"Check-Out    : <strong>{booking.CheckOutDate:dd MMM yyyy}</strong>",
                        $"Nights       : <strong>{nights}</strong>",
                       })
               });

               _log.Add(result.Success
                   ? $"[Decorator:Email] ✓ Created email sent to {emailAddress}"
                   : $"[Decorator:Email] ✗ Created email failed: {result.Message}");
          }

          private async Task SendBookingConfirmedAsync(Booking booking)
          {
               string emailAddress = GetGuestEmail(booking.UserId);
               if (string.IsNullOrEmpty(emailAddress)) return;

               int nights = (booking.CheckOutDate - booking.CheckInDate).Days;
               var result = await _gmail.SendAsync(new EmailMessage
               {
                    To = emailAddress,
                    Subject = $"[Grand Horizon] Booking CONFIRMED — #{booking.BookingId[..8]}",
                    IsHtml = true,
                    Body = BuildHtml(
                       title: "Booking Confirmed ✓",
                       colour: "#15803D",
                       icon: "✅",
                       lines: new[]
                       {
                        $"Booking ID   : <strong>{booking.BookingId[..8]}…</strong>",
                        $"Type         : <strong>{booking.BookingType}</strong>",
                        $"Status       : <strong style='color:#15803D'>CONFIRMED</strong>",
                        $"Check-In     : <strong>{booking.CheckInDate:dd MMM yyyy}</strong>",
                        $"Check-Out    : <strong>{booking.CheckOutDate:dd MMM yyyy}</strong>",
                        $"Nights       : <strong>{nights}</strong>",
                       })
               });

               _log.Add(result.Success
                   ? $"[Decorator:Email] ✓ Confirmed email sent to {emailAddress}"
                   : $"[Decorator:Email] ✗ Confirmed email failed: {result.Message}");
          }

          private async Task SendBookingCancelledAsync(Booking booking)
          {
               string emailAddress = GetGuestEmail(booking.UserId);
               if (string.IsNullOrEmpty(emailAddress)) return;

               var result = await _gmail.SendAsync(new EmailMessage
               {
                    To = emailAddress,
                    Subject = $"[Grand Horizon] Booking CANCELLED — #{booking.BookingId[..8]}",
                    IsHtml = true,
                    Body = BuildHtml(
                       title: "Booking Cancelled",
                       colour: "#DC2626",
                       icon: "❌",
                       lines: new[]
                       {
                        $"Booking ID   : <strong>{booking.BookingId[..8]}…</strong>",
                        $"Type         : <strong>{booking.BookingType}</strong>",
                        $"Status       : <strong style='color:#DC2626'>CANCELLED</strong>",
                        $"Originally   : {booking.CheckInDate:dd MMM yyyy} → {booking.CheckOutDate:dd MMM yyyy}",
                       })
               });

               _log.Add(result.Success
                   ? $"[Decorator:Email] ✓ Cancelled email sent to {emailAddress}"
                   : $"[Decorator:Email] ✗ Cancelled email failed: {result.Message}");
          }

          // ── Shared HTML template ───────────────────────────────────────────────
          private static string BuildHtml(string title, string colour, string icon, string[] lines)
          {
               var rows = new System.Text.StringBuilder();
               foreach (var l in lines)
                    rows.AppendLine($"<tr><td style='padding:6px 14px;border-bottom:1px solid #E2E8F0'>{l}</td></tr>");

               return $@"<!DOCTYPE html>
<html lang='en'>
<head><meta charset='UTF-8'/></head>
<body style='font-family:Segoe UI,Arial,sans-serif;margin:0;padding:32px;background:#F8FAFC'>
  <div style='max-width:520px;margin:auto;background:white;border-radius:12px;
              box-shadow:0 4px 20px rgba(0,0,0,.08);overflow:hidden'>
    <div style='background:{colour};padding:24px 28px;color:white'>
      <div style='font-size:28px;margin-bottom:4px'>{icon}</div>
      <h2 style='margin:0;font-size:20px'>{title}</h2>
      <p  style='margin:4px 0 0;opacity:.85;font-size:12px'>
          Grand Horizon Hotel Property Management System
      </p>
    </div>
    <table style='width:100%;border-collapse:collapse;font-size:13px;color:#374151'>
      {rows}
    </table>
    <div style='padding:16px 28px;background:#F8FAFC;font-size:11px;color:#9CA3AF'>
      Generated automatically by Grand Horizon Hotel PMS on {DateTime.Now:dd MMM yyyy HH:mm:ss}
    </div>
  </div>
</body>
</html>";
          }
     }
}