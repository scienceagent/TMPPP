using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

namespace HotelBookingSystem.Models
{
     public class DeluxeRoom : Room
     {
          public override int Capacity { get; protected set; }

          // Stored in DB as comma-separated string
          public string AmenitiesJson { get; private set; }

          [NotMapped]
          public IReadOnlyList<string> Amenities =>
              string.IsNullOrEmpty(AmenitiesJson)
                  ? new List<string>().AsReadOnly()
                  : AmenitiesJson.Split(',').ToList().AsReadOnly();

          public bool HasBalcony { get; private set; }

          private DeluxeRoom() { } // EF Core

          public DeluxeRoom(string roomId, string roomNumber, decimal basePrice,
                            int capacity, List<string> amenities, bool hasBalcony)
              : base(roomId, roomNumber, basePrice)
          {
               Capacity = capacity;
               AmenitiesJson = string.Join(",", amenities ?? new List<string>());
               HasBalcony = hasBalcony;
               ImagePath = "/Images/stubaileyphoto-bedroom-5772286.jpg";
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