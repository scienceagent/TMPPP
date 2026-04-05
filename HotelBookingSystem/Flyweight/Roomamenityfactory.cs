using System.Collections.Generic;

namespace HotelBookingSystem.Flyweight
{
     /// <summary>
     /// FlyweightFactory — maintains an internal cache.
     /// Returns an existing flyweight if the key exists, otherwise creates one and stores it.
     /// This is the core of the Flyweight pattern: N rooms share M amenity objects (M &lt;&lt; N).
     /// </summary>
     public class RoomAmenityFactory
     {
          private readonly Dictionary<string, IRoomAmenityFlyweight> _cache = new();

          // Pre-registered amenity definitions (intrinsic state for well-known amenity types)
          private static readonly Dictionary<string, (string Icon, string Color, string Category)> _definitions = new()
          {
               ["WiFi"] = ("📶", "#0277BD", "Connectivity"),
               ["Pool"] = ("🏊", "#00838F", "Wellness"),
               ["Gym"] = ("💪", "#558B2F", "Wellness"),
               ["Spa"] = ("🧖", "#AD1457", "Wellness"),
               ["Minibar"] = ("🍾", "#E65100", "Dining"),
               ["Breakfast"] = ("🍳", "#F57F17", "Dining"),
               ["Balcony"] = ("🌅", "#1565C0", "View"),
               ["Sea View"] = ("🌊", "#0288D1", "View"),
               ["Parking"] = ("🚗", "#455A64", "Service"),
               ["Airport Shuttle"] = ("✈", "#4527A0", "Service"),
               ["Room Service"] = ("🛎", "#BF360C", "Service"),
               ["Air Conditioning"] = ("❄", "#1976D2", "Comfort"),
               ["Fireplace"] = ("🔥", "#D84315", "Comfort"),
               ["Bathtub"] = ("🛁", "#6A1B9A", "Comfort"),
               ["Kitchen"] = ("🍽", "#2E7D32", "Dining"),
               ["Concierge"] = ("🎩", "#212121", "Service"),
          };

          /// <summary>
          /// Returns a shared flyweight for the given amenity type.
          /// Creates and caches it on first access.
          /// </summary>
          public IRoomAmenityFlyweight GetOrCreate(string amenityType)
          {
               if (_cache.TryGetValue(amenityType, out var existing))
                    return existing;   // ← returns SAME INSTANCE as before

               // Create new flyweight with intrinsic state
               if (_definitions.TryGetValue(amenityType, out var def))
               {
                    var flyweight = new RoomAmenityFlyweight(amenityType, def.Icon, def.Color, def.Category);
                    _cache[amenityType] = flyweight;
                    return flyweight;
               }

               // Unknown amenity type — create a generic flyweight
               var generic = new RoomAmenityFlyweight(amenityType, "★", "#757575", "Other");
               _cache[amenityType] = generic;
               return generic;
          }

          public int CacheSize => _cache.Count;

          public IReadOnlyDictionary<string, IRoomAmenityFlyweight> GetCache()
              => _cache;

          public IReadOnlyCollection<string> GetAvailableAmenityTypes()
              => _definitions.Keys;
     }
}