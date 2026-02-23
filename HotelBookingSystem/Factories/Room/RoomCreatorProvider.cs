using System;
using System.Collections.Generic;

namespace HotelBookingSystem.Factories
{
     public sealed class RoomCreatorProvider
     {
          private readonly Dictionary<string, Func<RoomCreator>> _registry = new Dictionary<string, Func<RoomCreator>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Standard", () => new StandardRoomCreator() },
            { "Deluxe",   () => new DeluxeRoomCreator()   },
            { "Suite",    () => new SuiteRoomCreator()    },
        };

          public RoomCreator GetCreator(string roomType)
          {
               if (_registry.TryGetValue(roomType, out var build))
                    return build();

               throw new ArgumentException($"Unknown room type: {roomType}");
          }

          public IReadOnlyList<string> GetAvailableTypes() => new List<string>(_registry.Keys);
     }
}