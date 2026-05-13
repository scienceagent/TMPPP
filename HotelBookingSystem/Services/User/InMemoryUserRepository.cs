using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Data;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.Services
{
     public class InMemoryUserRepository : IUserRepository
     {
          public User FindById(string id)
          {
              using var context = new AppDbContext();
              return context.Users.FirstOrDefault(u => u.Id == id);
          }

          public IEnumerable<User> GetAll()
          {
              using var context = new AppDbContext();
              return context.Users.ToList();
          }

          public void Save(User user)
          {
              using var context = new AppDbContext();
              var existing = context.Users.FirstOrDefault(u => u.Id == user.Id);
              if (existing != null)
              {
                  context.Entry(existing).CurrentValues.SetValues(user);
              }
              else
              {
                  context.Users.Add(user);
              }
              context.SaveChanges();
          }
     }
}