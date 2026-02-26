using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IBookingFactory
     {
          Booking CreateBooking(string bookingId, string userId, string roomId,
                                            DateTime checkIn, DateTime checkOut, string bookingType);
          IPricingStrategy CreatePricingStrategy();
          IConfirmationHandler CreateConfirmationHandler();
     }
}