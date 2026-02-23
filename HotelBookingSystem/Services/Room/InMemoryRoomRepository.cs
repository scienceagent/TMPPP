using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class InMemoryRoomRepository : IRoomRepository
     {
          private readonly List<Room> _rooms = new List<Room>();

          public Room FindById(string id) =>
              _rooms.FirstOrDefault(r => r.RoomId == id);

          public List<Room> GetAvailableRooms() =>
              _rooms.Where(r => r.IsAvailable).ToList();

          public List<Room> GetAllRooms() => new List<Room>(_rooms);

          public void Save(Room room)
          {
               var existing = FindById(room.RoomId);
               if (existing != null) _rooms.Remove(existing);
               _rooms.Add(room);
          }
     }
}