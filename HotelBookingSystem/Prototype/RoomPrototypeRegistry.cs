using System;
using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Prototype
{
     // ─── PROTOTYPE REGISTRY ──────────────────────────────────────────
     // Stores pre-configured room templates.
     // GetClone() ALWAYS returns a clone — original is never exposed.
     // Matches GoF: the registry is the "manager" that stores prototypes.
     public partial class RoomPrototypeRegistry
     {
          private readonly Dictionary<string, RoomPrototype> _prototypes = new();

          public RoomPrototypeRegistry()
          {
               // Pre-configured templates — set up once, cloned many times
               Register("Standard", new StandardRoomPrototype
               {
                    RoomNumber = "TEMPLATE",
                    BasePrice = 150m,
                    Capacity = 2,
                    IsAvailable = true
               });

               Register("Deluxe", new DeluxeRoomPrototype
               {
                    RoomNumber = "TEMPLATE",
                    BasePrice = 280m,
                    Capacity = 2,
                    HasBalcony = true,
                    IsAvailable = true,
                    Amenities = new List<string> { "Minibar", "Balcony", "Sea View" }
               });

               Register("Suite", new SuitePrototype
               {
                    RoomNumber = "TEMPLATE",
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

          // Returns a CLONE — original prototype is never modified
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