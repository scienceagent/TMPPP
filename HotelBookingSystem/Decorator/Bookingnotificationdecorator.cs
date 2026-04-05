using HotelBookingSystem.Models;

namespace HotelBookingSystem.Decorator
{
     /// <summary>
     /// Base Decorator — implements the component interface and holds a reference
     /// to the wrapped component (could be the concrete service or another decorator).
     /// Subclasses call base.Notify*() to forward to the inner component.
     /// </summary>
     public abstract class BookingNotificationDecorator : IBookingNotificationService
     {
          protected readonly IBookingNotificationService _inner;

          protected BookingNotificationDecorator(IBookingNotificationService inner)
          {
               _inner = inner;
          }

          public virtual void NotifyBookingCreated(Booking booking)
              => _inner.NotifyBookingCreated(booking);

          public virtual void NotifyBookingConfirmed(Booking booking)
              => _inner.NotifyBookingConfirmed(booking);

          public virtual void NotifyBookingCancelled(Booking booking)
              => _inner.NotifyBookingCancelled(booking);
     }
}