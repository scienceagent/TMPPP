using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Decorator
{
     /// <summary>
     /// Decorator 3 — SMS Notification.
     /// Only fires for confirmed or cancelled bookings (not every creation).
     /// Demonstrates that decorators can selectively add behavior.
     /// </summary>
     public class SmsNotificationDecorator : BookingNotificationDecorator
     {
          private readonly List<string> _log;
          private readonly string _smsGateway;

          public SmsNotificationDecorator(IBookingNotificationService inner,
                                          List<string> log,
                                          string smsGateway = "sms.hotel.md")
              : base(inner)
          {
               _log = log;
               _smsGateway = smsGateway;
          }

          // No override for NotifyBookingCreated — SMS not sent on creation (by design)

          public override void NotifyBookingConfirmed(Booking booking)
          {
               _log.Add($"[Decorator:SMS]   → {_smsGateway}: 'Confirmed: Check-in {booking.CheckInDate:dd MMM}, Room ready!'");
               _inner.NotifyBookingConfirmed(booking);
          }

          public override void NotifyBookingCancelled(Booking booking)
          {
               _log.Add($"[Decorator:SMS]   → {_smsGateway}: 'Booking cancelled. Contact us: +373 22 000 000'");
               _inner.NotifyBookingCancelled(booking);
          }
     }
}