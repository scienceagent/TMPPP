using System;
using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Decorator
{
     /// <summary>
     /// Decorator 1 — Logging.
     /// Adds audit-log entries before and after every notification.
     /// Single Responsibility: only knows about logging, nothing else.
     /// </summary>
     public class LoggingNotificationDecorator : BookingNotificationDecorator
     {
          private readonly List<string> _log;

          public LoggingNotificationDecorator(IBookingNotificationService inner, List<string> log)
              : base(inner)
          {
               _log = log;
          }

          public override void NotifyBookingCreated(Booking booking)
          {
               _log.Add($"[Decorator:Log] {DateTime.Now:HH:mm:ss} NotifyBookingCreated  → {booking.BookingId[..8]}... ({booking.BookingType})");
               _inner.NotifyBookingCreated(booking);
          }

          public override void NotifyBookingConfirmed(Booking booking)
          {
               _log.Add($"[Decorator:Log] {DateTime.Now:HH:mm:ss} NotifyBookingConfirmed → {booking.BookingId[..8]}...");
               _inner.NotifyBookingConfirmed(booking);
          }

          public override void NotifyBookingCancelled(Booking booking)
          {
               _log.Add($"[Decorator:Log] {DateTime.Now:HH:mm:ss} NotifyBookingCancelled → {booking.BookingId[..8]}...");
               _inner.NotifyBookingCancelled(booking);
          }
     }
}