using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Flyweight
{
     /// <summary>
     /// Represents one room's amenity listing entry.
     /// Stores ONLY extrinsic state — the shared flyweight is referenced, never copied.
     /// </summary>
     public class RoomAmenityEntry
     {
          // Extrinsic state (unique per room)
          public string RoomId { get; }
          public string RoomNumber { get; }
          public decimal RoomPrice { get; }

          // Reference to shared flyweight (intrinsic state lives there)
          public IRoomAmenityFlyweight Flyweight { get; }

          public RoomAmenityEntry(string roomId, string roomNumber, decimal roomPrice,
                                  IRoomAmenityFlyweight flyweight)
          {
               RoomId = roomId;
               RoomNumber = roomNumber;
               RoomPrice = roomPrice;
               Flyweight = flyweight;
          }

          /// <summary>
          /// Renders the amenity badge — delegates to flyweight,
          /// supplying extrinsic state (roomId, price) as parameters.
          /// </summary>
          public string Render() => Flyweight.Render(RoomId, RoomPrice);
     }

     /// <summary>
     /// Client — creates RoomAmenityEntry objects (each holds extrinsic state)
     /// but gets the shared flyweight objects from the factory.
     /// Simulates loading many rooms with many amenities — the memory savings come
     /// from having only one flyweight per unique amenity type regardless of room count.
     /// </summary>
     public class RoomAmenityRenderer
     {
          private readonly RoomAmenityFactory _factory;
          private readonly List<RoomAmenityEntry> _entries = new();

          public RoomAmenityRenderer(RoomAmenityFactory factory)
          {
               _factory = factory;
          }

          /// <summary>
          /// Register a room's amenity. Gets/creates a shared flyweight for the amenity type.
          /// </summary>
          public void AddRoomAmenity(string roomId, string roomNumber, decimal price, string amenityType)
          {
               var flyweight = _factory.GetOrCreate(amenityType);
               _entries.Add(new RoomAmenityEntry(roomId, roomNumber, price, flyweight));
          }

          public IReadOnlyList<RoomAmenityEntry> GetEntries() => _entries.AsReadOnly();

          public string GetReport()
          {
               var sb = new StringBuilder();
               sb.AppendLine($"=== Room Amenity Report: {_entries.Count} entries, {_factory.CacheSize} flyweight objects ===");
               sb.AppendLine($"Memory saved: {_entries.Count} entry objects share {_factory.CacheSize} flyweight instances.");
               sb.AppendLine();

               foreach (var e in _entries)
                    sb.AppendLine($"  {e.Render()}");

               return sb.ToString();
          }
     }
}