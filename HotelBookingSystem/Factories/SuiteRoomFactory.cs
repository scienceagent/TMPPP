using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public class SuiteRoomFactory : IRoomFactory
     {
          private readonly bool _hasKitchen;
          private readonly bool _hasLivingRoom;

          public SuiteRoomFactory()
          {
               _hasKitchen = true;
               _hasLivingRoom = true;
          }

          public SuiteRoomFactory(bool hasKitchen, bool hasLivingRoom)
          {
               _hasKitchen = hasKitchen;
               _hasLivingRoom = hasLivingRoom;
          }

          public Room CreateRoom(string roomId, string roomNumber, decimal basePrice, int capacity)
          {
               return new Suite(roomId, roomNumber, basePrice, capacity, _hasKitchen, _hasLivingRoom);
          }
     }
}