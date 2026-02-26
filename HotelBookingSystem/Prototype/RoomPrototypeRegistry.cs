using System;
using System.Collections.Generic;
using System.Globalization;

namespace HotelBookingSystem.Prototype
{
     public class RoomPrototypeRegistry
     {
          // Each entry stores the clone operation so the original is never exposed
          private readonly Dictionary<string, Func<RoomTemplateSnapshot>> _registry = new();

          public RoomPrototypeRegistry()
          {
               var standard = new StandardRoomPrototype("", 150m, 2);
               var deluxe = new DeluxeRoomPrototype("", 280m, 2, true, ["Minibar", "Balcony", "Sea View"]);
               var suite = new SuitePrototype("", 500m, 4, true, true, 2);

               _registry["Standard"] = () =>
               {
                    var c = standard.Clone();
                    return new RoomTemplateSnapshot(c.RoomNumber, c.BasePrice, c.Capacity, c.GetDisplayInfo());
               };

               _registry["Deluxe"] = () =>
               {
                    var c = deluxe.Clone();
                    return new RoomTemplateSnapshot(c.RoomNumber, c.BasePrice, c.Capacity, c.GetDisplayInfo());
               };

               _registry["Suite"] = () =>
               {
                    var c = suite.Clone();
                    return new RoomTemplateSnapshot(c.RoomNumber, c.BasePrice, c.Capacity, c.GetDisplayInfo());
               };
          }

          public RoomTemplateSnapshot GetClone(string key)
          {
               if (!_registry.TryGetValue(key, out var cloneFunc))
                    throw new KeyNotFoundException($"No prototype registered for: '{key}'");

               return cloneFunc();
          }

          public IReadOnlyCollection<string> GetAvailableTypes() => [.. _registry.Keys];
     }

     // Snapshot returned to callers — decoupled from the prototype internals
     public class RoomTemplateSnapshot
     {
          public string RoomNumber { get; set; }
          public decimal BasePrice { get; set; }
          public int Capacity { get; set; }
          public string DisplayInfo { get; }

          public RoomTemplateSnapshot(string roomNumber, decimal basePrice, int capacity, string displayInfo)
          {
               RoomNumber = roomNumber;
               BasePrice = basePrice;
               Capacity = capacity;
               DisplayInfo = displayInfo;
          }
     }
}