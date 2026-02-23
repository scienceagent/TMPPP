using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class RoomPricingService : IRoomPricingService
     {
          public decimal CalculatePrice(Room room)
          {
               switch (room)
               {
                    case Suite suite:
                         return suite.BasePrice
                              + suite.NumberOfRooms * 100m
                              + (suite.HasKitchen ? 150m : 0m)
                              + (suite.HasLivingRoom ? 100m : 0m);

                    case DeluxeRoom deluxe:
                         return deluxe.BasePrice
                              + deluxe.Amenities.Count * 20m
                              + (deluxe.HasBalcony ? 50m : 0m);

                    default:
                         return room.BasePrice;
               }
          }

          public decimal CalculateCleaningCost(Room room)
          {
               switch (room)
               {
                    case Suite suite:
                         return 150m + suite.NumberOfRooms * 30m;

                    case DeluxeRoom _:
                         return 80m;

                    default:
                         return 50m;
               }
          }
     }
}