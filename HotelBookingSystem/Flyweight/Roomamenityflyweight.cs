namespace HotelBookingSystem.Flyweight
{
     /// <summary>
     /// Concrete Flyweight — stores ONLY intrinsic (shared, immutable) state.
     /// One instance of this class is shared by all rooms that have this amenity type.
     /// If 200 rooms all have "WiFi", there is exactly ONE RoomAmenityFlyweight("WiFi") object.
     /// </summary>
     public class RoomAmenityFlyweight : IRoomAmenityFlyweight
     {
          // ── INTRINSIC STATE (shared, immutable) ──────────────────────────────
          public string AmenityType { get; }
          public string Icon { get; }
          public string Color { get; }
          public string Category { get; }

          public RoomAmenityFlyweight(string amenityType, string icon, string color, string category)
          {
               AmenityType = amenityType;
               Icon = icon;
               Color = color;
               Category = category;
          }

          // ── OPERATION — uses intrinsic state + extrinsic params ───────────────
          public string Render(string roomId, decimal roomPrice)
          {
               // Extrinsic state (roomId, roomPrice) is passed in — NOT stored here
               return $"[{Icon} {AmenityType}] Room:{roomId} @${roomPrice:F0} ({Category})";
          }
     }
}