using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Prototype
{
     public class RoomPrototypeRegistry
     {
          private readonly Dictionary<string, RoomPrototype> _prototypes = new();

          public RoomPrototypeRegistry()
          {
               Register("Standard", new StandardRoomPrototype
               {
                    BasePrice = 150m,
                    Capacity = 2,
                    IsAvailable = true
               });

               Register("Deluxe", new DeluxeRoomPrototype
               {
                    BasePrice = 280m,
                    Capacity = 2,
                    HasBalcony = true,
                    IsAvailable = true,
                    Amenities = new List<string> { "Minibar", "Balcony", "Sea View" }
               });

               Register("Suite", new SuitePrototype
               {
                    BasePrice = 500m,
                    Capacity = 4,
                    HasKitchen = true,
                    HasLivingRoom = true,
                    NumberOfRooms = 2,
                    IsAvailable = true
               });
          }

          public void Register(string key, RoomPrototype prototype)
              => _prototypes[key] = prototype;

          public RoomPrototype GetClone(string key)
          {
               if (!_prototypes.TryGetValue(key, out var prototype))
                    throw new KeyNotFoundException($"No prototype registered for: '{key}'");

               return prototype.Clone();
          }

          public IReadOnlyCollection<string> GetAvailableTypes()
              => _prototypes.Keys;
     }
}