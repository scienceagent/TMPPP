using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IBookingService
     {
          BookingResult CreateBooking(Booking booking);
          BookingResult ConfirmBooking(string bookingId);
          BookingResult CancelBooking(string bookingId);
     }
}