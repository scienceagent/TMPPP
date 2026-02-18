namespace HotelBookingSystem.Models
{
     public abstract class Room
     {
          public string RoomId { get; }
          public string RoomNumber { get; }
          public decimal BasePrice { get; }
          public bool IsAvailable { get; private set; }
          public abstract int Capacity { get; }

          protected Room(string roomId, string roomNumber, decimal basePrice)
          {
               RoomId = roomId;
               RoomNumber = roomNumber;
               BasePrice = basePrice;
               IsAvailable = true;
          }

          public void SetAvailability(bool status) => IsAvailable = status;

          // Virtual — subclasses CAN override to provide richer details
          public virtual string GetDisplayInfo() => $"Room {RoomNumber} | Capacity: {Capacity}";

          public virtual string GetDescription() => $"Standard accommodation in room {RoomNumber}.";
     }
}