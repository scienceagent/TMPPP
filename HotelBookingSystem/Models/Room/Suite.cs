using System.Globalization;

namespace HotelBookingSystem.Models
{
     public class Suite : Room
     {
          public int NumberOfRooms { get; private set; }
          public bool HasKitchen { get; private set; }
          public bool HasLivingRoom { get; private set; }
          public override int Capacity { get; protected set; }

          private Suite() { } // EF Core

          public Suite(string roomId, string roomNumber, decimal basePrice,
                       int numberOfRooms, bool hasKitchen, bool hasLivingRoom)
              : base(roomId, roomNumber, basePrice)
          {
               NumberOfRooms = numberOfRooms;
               HasKitchen = hasKitchen;
               HasLivingRoom = hasLivingRoom;
               Capacity = NumberOfRooms * 2;
               ImagePath = "/Images/3534679-bedroom-3475656.jpg";
          }

          public override void SetAvailability(bool status) => IsAvailable = status;

          public override string GetDisplayInfo()
          {
               var kitchen = HasKitchen ? "with kitchen" : "no kitchen";
               var living = HasLivingRoom ? "with living room" : "no living room";
               return $"Suite {RoomNumber} | {NumberOfRooms} rooms, {kitchen}, {living} | Capacity: {Capacity}";
          }

          public override string GetDescription() =>
               $"Luxury suite with {NumberOfRooms} rooms accommodating up to {Capacity} guests.";

          public override string GetPriceSummary(decimal price) =>
               $"Price: {price.ToString("C", CultureInfo.GetCultureInfo("en-US"))} ({NumberOfRooms} rooms x premium rate)";
     }
}