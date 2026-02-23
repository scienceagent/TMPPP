using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IRoomRepository
     {
          Room FindById(string id);
          List<Room> GetAvailableRooms();
          List<Room> GetAllRooms();
          void Save(Room room);
     }
}