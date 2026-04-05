using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Decorator
{
     /// <summary>
     /// Decorator 2 — Email Notification.
     /// Simulates sending an email to the guest after each event.
     /// Knows nothing about logging or SMS — single responsibility.
     /// </summary>
     public class EmailNotificationDecorator : BookingNotificationDecorator
     {
          private readonly List<string> _log;
          private readonly string _smtpServer;

          public EmailNotificationDecorator(IBookingNotificationService inner,
                                            List<string> log,
                                            string smtpServer = "smtp.hotel.md")
              : base(inner)
          {
               _log = log;
               _smtpServer = smtpServer;
          }

          public override void NotifyBookingCreated(Booking booking)
          {
               _log.Add($"[Decorator:Email] → SMTP({_smtpServer}) Subject: 'Booking #{booking.BookingId[..8]}... Created — {booking.BookingType} Package'");
               _inner.NotifyBookingCreated(booking);
          }

          public override void NotifyBookingConfirmed(Booking booking)
          {
               _log.Add($"[Decorator:Email] → SMTP({_smtpServer}) Subject: 'Your Booking is Confirmed ✓ — CheckIn {booking.CheckInDate:dd MMM yyyy}'");
               _inner.NotifyBookingConfirmed(booking);
          }

          public override void NotifyBookingCancelled(Booking booking)
          {
               _log.Add($"[Decorator:Email] → SMTP({_smtpServer}) Subject: 'Booking Cancelled — Refund Processing'");
               _inner.NotifyBookingCancelled(booking);
          }
     }
}