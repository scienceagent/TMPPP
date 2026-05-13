using System.Globalization;

namespace HotelBookingSystem.Models
{
     public class StandardRoom : Room
     {
          public override int Capacity { get; protected set; }

          private StandardRoom() { } // EF Core

          public StandardRoom(string roomId, string roomNumber, decimal basePrice, int capacity)
              : base(roomId, roomNumber, basePrice)
          {
               Capacity = capacity;
               ImagePath = "/Images/4506458-room-2269591_1920.jpg";
          }

          public override void SetAvailability(bool status) => IsAvailable = status;
          public override string GetDisplayInfo() => $"Standard Room {RoomNumber} | Capacity: {Capacity}";
          public override string GetDescription() => $"Comfortable standard room {RoomNumber} for up to {Capacity} guests.";
          public override string GetPriceSummary(decimal price) =>
              $"Price: {price.ToString("C", CultureInfo.GetCultureInfo("en-US"))} (standard rate)";
     }
}