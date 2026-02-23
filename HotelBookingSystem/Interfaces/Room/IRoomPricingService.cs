using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IRoomPricingService
     {
          decimal CalculatePrice(Room room);
          decimal CalculateCleaningCost(Room room);
     }
}