using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public sealed class StandardRoomCreator : RoomCreator
     {
          public override IRoomProduct CreateProduct(string roomId, string roomNumber, decimal basePrice, int capacity)
              => new StandardRoom(roomId, roomNumber, basePrice, capacity);
     }
}