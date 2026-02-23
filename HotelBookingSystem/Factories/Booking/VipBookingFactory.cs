using System;
using HotelBookingSystem.Factories.Confirmation;
using HotelBookingSystem.Factories.Pricing;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public class VipBookingFactory : IBookingFactory
     {
          public Booking CreateBooking(string bookingId, string userId, string roomId, DateTime checkIn, DateTime checkOut)
              => new Booking(bookingId, userId, roomId, checkIn, checkOut);

          public IPricingStrategy CreatePricingStrategy()
              => new VipPricingStrategy();

          public IConfirmationHandler CreateConfirmationHandler()
              => new VipConfirmationHandler();
     }
}