using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public abstract class RoomCreator
     {
          public abstract IRoomProduct CreateProduct(string roomId, string roomNumber,
                                                     decimal basePrice, int capacity);

          public (bool success, string error, Room room,
                  string display, string description,
                  string priceSummary, decimal cleaningCost)
              CreateRoom(string roomNumber, decimal basePrice, int capacity,
                         IRoomRepository repository, IRoomPricingService pricingService)
          {
               if (string.IsNullOrWhiteSpace(roomNumber))
                    return (false, "Room number is required.", null, null, null, null, 0);
               if (basePrice <= 0)
                    return (false, "Base price must be greater than zero.", null, null, null, null, 0);
               if (capacity <= 0)
                    return (false, "Capacity must be greater than zero.", null, null, null, null, 0);

               var product = CreateProduct(Guid.NewGuid().ToString(), roomNumber, basePrice, capacity);
               var room = (Room)product;

               repository.Save(room);

               decimal price = pricingService.CalculatePrice(room);
               decimal cleaning = pricingService.CalculateCleaningCost(room);

               return (true, null, room,
                       product.GetDisplayInfo(),
                       product.GetDescription(),
                       product.GetPriceSummary(price),
                       cleaning);
          }
     }
}