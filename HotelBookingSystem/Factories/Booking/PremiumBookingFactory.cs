using System;
using HotelBookingSystem.Factories.Confirmation;
using HotelBookingSystem.Factories.Pricing;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public class PremiumBookingFactory : IBookingFactory
     {
          public Booking CreateBooking(string bookingId, string userId, string roomId,
                                       DateTime checkIn, DateTime checkOut, string bookingType)
              => new Booking(bookingId, userId, roomId, checkIn, checkOut, bookingType);

          public IPricingStrategy CreatePricingStrategy() => new PremiumPricingStrategy();
          public IConfirmationHandler CreateConfirmationHandler() => new PremiumConfirmationHandler();
     }
}