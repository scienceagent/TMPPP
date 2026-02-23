using System;
using System.Collections.Generic;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Factories
{
     public class BookingFactoryProvider
     {
          private readonly Dictionary<string, Func<IBookingFactory>> _registry =
              new Dictionary<string, Func<IBookingFactory>>(StringComparer.OrdinalIgnoreCase)
              {
                { "Standard", () => new StandardBookingFactory() },
                { "Premium",  () => new PremiumBookingFactory()  },
                { "VIP",      () => new VipBookingFactory()      },
              };

          public IBookingFactory GetFactory(string bookingType)
          {
               if (_registry.TryGetValue(bookingType, out var build))
                    return build();

               throw new ArgumentException($"Unknown booking type: {bookingType}");
          }

          public IReadOnlyList<string> GetAvailableTypes() => new List<string>(_registry.Keys);
     }
}