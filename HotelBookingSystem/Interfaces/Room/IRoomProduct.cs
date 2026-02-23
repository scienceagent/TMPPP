namespace HotelBookingSystem.Interfaces
{
     public interface IRoomProduct
     {
          string RoomId { get; }
          string RoomNumber { get; }
          decimal BasePrice { get; }
          bool IsAvailable { get; }
          int Capacity { get; }

          string GetDisplayInfo();
          string GetDescription();
          string GetPriceSummary(decimal calculatedPrice);
          void SetAvailability(bool status);
     }
}