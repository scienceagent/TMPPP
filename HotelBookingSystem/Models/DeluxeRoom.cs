using System.Collections.Generic;

namespace HotelBookingSystem.Models
{
     public class DeluxeRoom : Room
     {
          public override int Capacity { get; }
          public IReadOnlyList<string> Amenities { get; }
          public bool HasBalcony { get; }

          public DeluxeRoom(string roomId, string roomNumber, decimal basePrice,
                            int capacity, List<string> amenities, bool hasBalcony)
              : base(roomId, roomNumber, basePrice)
          {
               Capacity = capacity;
               Amenities = (amenities ?? new List<string>()).AsReadOnly();
               HasBalcony = hasBalcony;
          }

          public override string GetDisplayInfo()
          {
               var balcony = HasBalcony ? "with balcony" : "no balcony";
               return $"Deluxe Room {RoomNumber} | {balcony} | Capacity: {Capacity}";
          }

          public override string GetDescription()
          {
               var amenitiesList = string.Join(", ", Amenities);
               return $"Deluxe room featuring: {amenitiesList}.";
          }
     }
}