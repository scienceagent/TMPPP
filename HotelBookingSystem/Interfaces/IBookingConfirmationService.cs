using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IBookingConfirmationService
     {
          void ConfirmBooking(Booking booking, Room room);
          void CancelBooking(Booking booking, Room room);
          void CompleteBooking(Booking booking, Room room);
     }
}