using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public sealed class SuiteRoomCreator : RoomCreator
     {
          private readonly bool _hasKitchen;
          private readonly bool _hasLivingRoom;

          public SuiteRoomCreator()
          {
               _hasKitchen = true;
               _hasLivingRoom = true;
          }

          public SuiteRoomCreator(bool hasKitchen, bool hasLivingRoom)
          {
               _hasKitchen = hasKitchen;
               _hasLivingRoom = hasLivingRoom;
          }

          public override IRoomProduct CreateProduct(string roomId, string roomNumber, decimal basePrice, int capacity)
              => new Suite(roomId, roomNumber, basePrice, capacity, _hasKitchen, _hasLivingRoom);
     }
}