using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IRoomFactory
     {
          Room CreateRoom(string roomId, string roomNumber, decimal basePrice, int capacity);
     }
}