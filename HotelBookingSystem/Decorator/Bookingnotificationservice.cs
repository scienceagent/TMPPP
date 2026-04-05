using HotelBookingSystem.Models;

namespace HotelBookingSystem.Decorator
{
     /// <summary>
     /// Concrete Component — the base implementation.
     /// Sends a simple in-app notification (writes to audit log).
     /// Decorators will wrap this to add logging, email, SMS etc.
     /// </summary>
     public class BookingNotificationService : IBookingNotificationService
     {
          public void NotifyBookingCreated(Booking booking)
          {
               System.Diagnostics.Debug.WriteLine(
                   $"[Notification:Core] Booking {booking.BookingId[..8]}... created ({booking.BookingType}).");
          }

          public void NotifyBookingConfirmed(Booking booking)
          {
               System.Diagnostics.Debug.WriteLine(
                   $"[Notification:Core] Booking {booking.BookingId[..8]}... confirmed.");
          }

          public void NotifyBookingCancelled(Booking booking)
          {
               System.Diagnostics.Debug.WriteLine(
                   $"[Notification:Core] Booking {booking.BookingId[..8]}... cancelled.");
          }
     }
}