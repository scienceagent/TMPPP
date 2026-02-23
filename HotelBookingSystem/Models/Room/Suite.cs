using System.Globalization;

namespace HotelBookingSystem.Models
{
     public class Suite : Room
     {
          public int NumberOfRooms { get; }
          public bool HasKitchen { get; }
          public bool HasLivingRoom { get; }
          public override int Capacity => NumberOfRooms * 2;

          public Suite(string roomId, string roomNumber, decimal basePrice,
                       int numberOfRooms, bool hasKitchen, bool hasLivingRoom)
              : base(roomId, roomNumber, basePrice)
          {
               NumberOfRooms = numberOfRooms;
               HasKitchen = hasKitchen;
               HasLivingRoom = hasLivingRoom;
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
              $"Price: {price.ToString("C", CultureInfo.GetCultureInfo("en-US"))} ({NumberOfRooms} rooms × premium rate)";
     }
}