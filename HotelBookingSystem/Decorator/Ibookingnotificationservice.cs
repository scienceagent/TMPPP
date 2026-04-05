using HotelBookingSystem.Models;

namespace HotelBookingSystem.Decorator
{
     /// <summary>
     /// Component interface — the contract that both the concrete service
     /// and all decorators must implement.
     /// Decorator adds behavior to this without subclassing or modifying it.
     /// </summary>
     public interface IBookingNotificationService
     {
          void NotifyBookingCreated(Booking booking);
          void NotifyBookingConfirmed(Booking booking);
          void NotifyBookingCancelled(Booking booking);
     }
}