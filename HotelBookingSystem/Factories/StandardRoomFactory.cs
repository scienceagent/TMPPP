using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public class StandardRoomFactory : IRoomFactory
     {
          public Room CreateRoom(string roomId, string roomNumber, decimal basePrice, int capacity)
          {
               return new StandardRoom(roomId, roomNumber, basePrice, capacity);
          }
     }
}