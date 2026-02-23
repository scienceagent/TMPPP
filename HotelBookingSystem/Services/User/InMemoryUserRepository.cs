using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.Services
{
     public class InMemoryUserRepository : IUserRepository
     {
          private readonly List<User> _users = new List<User>();

          public User FindById(string id) =>
              _users.FirstOrDefault(u => u.Id == id);

          public void Save(User user)
          {
               var existing = FindById(user.Id);
               if (existing != null) _users.Remove(existing);
               _users.Add(user);
          }
     }
}