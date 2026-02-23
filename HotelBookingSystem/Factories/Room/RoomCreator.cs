using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public abstract class RoomCreator
     {
          // Factory method
          public abstract IRoomProduct CreateProduct(string roomId, string roomNumber, decimal basePrice, int capacity);

          // someOperation() — business logic that uses the product
          public (bool success, string error, Room room, string display, string description, string priceSummary, decimal cleaningCost) CreateRoom(
              string roomNumber, decimal basePrice, int capacity,
              IRoomRepository repository, IRoomPricingService pricingService)
          {
               if (string.IsNullOrWhiteSpace(roomNumber))
                    return (false, "Room number is required.", null, null, null, null, 0);
               if (basePrice <= 0)
                    return (false, "Base price must be greater than zero.", null, null, null, null, 0);
               if (capacity <= 0)
                    return (false, "Capacity must be greater than zero.", null, null, null, null, 0);

               // "Product p = createProduct()"
               IRoomProduct p = CreateProduct(Guid.NewGuid().ToString(), roomNumber, basePrice, capacity);

               var room = (Room)p;
               repository.Save(room);

               decimal price = pricingService.CalculatePrice(room);
               decimal cleaning = pricingService.CalculateCleaningCost(room);

               return (true, null, room, p.GetDisplayInfo(), p.GetDescription(), p.GetPriceSummary(price), cleaning);
          }
     }
}