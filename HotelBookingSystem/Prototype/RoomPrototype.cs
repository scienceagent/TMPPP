using System;
using System.Collections.Generic;
using System.Globalization;

namespace HotelBookingSystem.Models
{
     // ─── PROTOTYPE ABSTRACT ──────────────────────────────────────────
     // Each room type knows how to clone itself.
     // Matches GoF: abstract Clone() — subclasses implement deep copy.
     public abstract class RoomPrototype
     {
          public string RoomId { get; set; } = string.Empty;
          public string RoomNumber { get; set; } = string.Empty;
          public decimal BasePrice { get; set; }
          public bool IsAvailable { get; set; } = true;
          public abstract int Capacity { get; set; }

          // GoF: abstract Clone() — every subclass copies itself completely
          public abstract RoomPrototype Clone();

          public virtual string GetDisplayInfo() =>
              $"Room {RoomNumber} | Price: {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | Capacity: {Capacity}";
     }

     // ─── CONCRETE PROTOTYPE: Standard ────────────────────────────────
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
              $"[Standard] Room {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | Capacity: {Capacity}";
     }

     // ─── CONCRETE PROTOTYPE: Deluxe ──────────────────────────────────
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
                   Amenities = new List<string>(this.Amenities)  // ← deep copy of list!
              };

          public override string GetDisplayInfo() =>
              $"[Deluxe] Room {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | Balcony: {HasBalcony} | Capacity: {Capacity}";
     }

     // ─── CONCRETE PROTOTYPE: Suite ───────────────────────────────────
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
              $"[Suite] Room {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | {NumberOfRooms} rooms | Kitchen: {HasKitchen} | Capacity: {Capacity}";
     }
}