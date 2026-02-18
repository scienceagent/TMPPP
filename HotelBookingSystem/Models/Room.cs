namespace HotelBookingSystem.Models
{
     public abstract class Room
     {
          public string RoomId { get; }
          public string RoomNumber { get; }
          public decimal BasePrice { get; }
          public bool IsAvailable { get; protected set; }
          public abstract int Capacity { get; }

          protected Room(string roomId, string roomNumber, decimal basePrice)
          {
               RoomId = roomId;
               RoomNumber = roomNumber;
               BasePrice = basePrice;
               IsAvailable = true;
          }

          public virtual void SetAvailability(bool status) => IsAvailable = status;

          public virtual string GetDisplayInfo() => $"Room {RoomNumber} | Capacity: {Capacity}";

          public virtual string GetDescription() => $"Standard accommodation in room {RoomNumber}.";
     }
}