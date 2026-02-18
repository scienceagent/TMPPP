using System;
using System.Collections.Generic;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Factories
{
     public class BookingFactoryProvider
     {
          private readonly Dictionary<string, IBookingFactory> _factories;

          public BookingFactoryProvider()
          {
               _factories = new Dictionary<string, IBookingFactory>
               {
                    { "Standard", new StandardBookingFactory() },
                    { "Premium", new PremiumBookingFactory() },
                    { "VIP", new VipBookingFactory() }
               };
          }

          public IBookingFactory GetFactory(string bookingType)
          {
               if (_factories.TryGetValue(bookingType, out var factory))
                    return factory;

               throw new ArgumentException($"Unknown booking type: {bookingType}");
          }

          public IReadOnlyList<string> GetAvailableTypes() =>
               new List<string>(_factories.Keys);
     }
}