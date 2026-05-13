using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Data;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class InMemoryRoomRepository : IRoomRepository
     {
          public Room FindById(string id)
          {
              using var context = new AppDbContext();
              return context.Rooms.FirstOrDefault(r => r.RoomId == id);
          }

          public List<Room> GetAvailableRooms()
          {
              using var context = new AppDbContext();
              return context.Rooms.Where(r => r.IsAvailable).ToList();
          }

          public List<Room> GetAllRooms()
          {
              using var context = new AppDbContext();
              return context.Rooms.ToList();
          }

          public void Save(Room room)
          {
              using var context = new AppDbContext();
              var existing = context.Rooms.FirstOrDefault(r => r.RoomId == room.RoomId);
              if (existing != null)
              {
                  context.Entry(existing).CurrentValues.SetValues(room);
              }
              else
              {
                  context.Rooms.Add(room);
              }
              context.SaveChanges();
          }
     }
}