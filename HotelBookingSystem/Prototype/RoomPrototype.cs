using System.Collections.Generic;
using System.Globalization;

namespace HotelBookingSystem.Models
{
     public abstract class RoomPrototype
     {
          public string RoomId { get; set; } = string.Empty;
          public string RoomNumber { get; set; } = string.Empty;
          public decimal BasePrice { get; set; }
          public bool IsAvailable { get; set; } = true;

          public abstract int Capacity { get; set; }
          public abstract RoomPrototype Clone();

          public virtual string GetDisplayInfo() =>
              $"Room {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | Capacity: {Capacity}";
     }

     public class StandardRoomPrototype : RoomPrototype
     {
          public override int Capacity { get; set; }

          public override RoomPrototype Clone() =>
              new StandardRoomPrototype
              {
                   RoomId = this.RoomId,
                   RoomNumber = this.RoomNumber,
                   BasePrice = this.BasePrice,
                   IsAvailable = this.IsAvailable,
                   Capacity = this.Capacity
              };

          public override string GetDisplayInfo() =>
              $"[Standard] {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | Capacity: {Capacity}";
     }

     public class DeluxeRoomPrototype : RoomPrototype
     {
          public override int Capacity { get; set; }
          public bool HasBalcony { get; set; }
          public List<string> Amenities { get; set; } = new();

          public override RoomPrototype Clone() =>
              new DeluxeRoomPrototype
              {
                   RoomId = this.RoomId,
                   RoomNumber = this.RoomNumber,
                   BasePrice = this.BasePrice,
                   IsAvailable = this.IsAvailable,
                   Capacity = this.Capacity,
                   HasBalcony = this.HasBalcony,
                   Amenities = new List<string>(this.Amenities)
              };

          public override string GetDisplayInfo() =>
              $"[Deluxe] {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | Balcony: {HasBalcony} | Capacity: {Capacity}";
     }

     public class SuitePrototype : RoomPrototype
     {
          public override int Capacity { get; set; }
          public bool HasKitchen { get; set; }
          public bool HasLivingRoom { get; set; }
          public int NumberOfRooms { get; set; }

          public override RoomPrototype Clone() =>
              new SuitePrototype
              {
                   RoomId = this.RoomId,
                   RoomNumber = this.RoomNumber,
                   BasePrice = this.BasePrice,
                   IsAvailable = this.IsAvailable,
                   Capacity = this.Capacity,
                   HasKitchen = this.HasKitchen,
                   HasLivingRoom = this.HasLivingRoom,
                   NumberOfRooms = this.NumberOfRooms
              };

          public override string GetDisplayInfo() =>
              $"[Suite] {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | {NumberOfRooms} rooms | Capacity: {Capacity}";
     }
}