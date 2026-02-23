using System.Collections.Generic;
using System.Globalization;

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

          public override void SetAvailability(bool status) => IsAvailable = status;

          public override string GetDisplayInfo()
          {
               var balcony = HasBalcony ? "with balcony" : "no balcony";
               return $"Deluxe Room {RoomNumber} | {balcony} | Capacity: {Capacity}";
          }

          public override string GetDescription() =>
              $"Deluxe room featuring: {string.Join(", ", Amenities)}.";

          public override string GetPriceSummary(decimal price) =>
              $"Price: {price.ToString("C", CultureInfo.GetCultureInfo("en-US"))} (includes {Amenities.Count} amenities{(HasBalcony ? " + balcony" : "")})";
     }
}