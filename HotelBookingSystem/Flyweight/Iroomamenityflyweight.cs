namespace HotelBookingSystem.Flyweight
{
     /// <summary>
     /// Flyweight interface — defines the operation that uses both intrinsic (shared) state
     /// and extrinsic (per-room) state passed as parameters.
     /// Intrinsic state: AmenityType, Icon, Color, Category (stored in the flyweight)
     /// Extrinsic state: roomId, roomNumber, price (provided by the caller per render)
     /// </summary>
     public interface IRoomAmenityFlyweight
     {
          string AmenityType { get; }  // intrinsic — e.g. "WiFi", "Pool", "Gym"
          string Icon { get; }         // intrinsic — Unicode icon character
          string Color { get; }        // intrinsic — hex color for UI badge
          string Category { get; }     // intrinsic — "Connectivity", "Wellness", "Dining"

          /// <summary>
          /// Renders a badge label using the shared intrinsic state
          /// plus room-specific extrinsic state (roomId, price).
          /// </summary>
          string Render(string roomId, decimal roomPrice);
     }
}