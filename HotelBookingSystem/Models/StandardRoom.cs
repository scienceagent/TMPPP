namespace HotelBookingSystem.Models
{
     public class StandardRoom : Room
     {
          public override int Capacity { get; }

          public StandardRoom(string roomId, string roomNumber, decimal basePrice, int capacity)
              : base(roomId, roomNumber, basePrice)
          {
               Capacity = capacity;
          }

          public override string GetDisplayInfo() =>
              $"Standard Room {RoomNumber} | Capacity: {Capacity}";

          public override string GetDescription() =>
              $"Comfortable standard room {RoomNumber} for up to {Capacity} guests.";
     }
}