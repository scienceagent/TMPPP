using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IBookingFactory
     {
          Booking CreateBooking(string bookingId, string userId, string roomId,
                                System.DateTime checkIn, System.DateTime checkOut);
          IPricingStrategy CreatePricingStrategy();
          IConfirmationHandler CreateConfirmationHandler();
     }
}