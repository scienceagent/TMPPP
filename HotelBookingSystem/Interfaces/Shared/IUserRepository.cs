using System.Collections.Generic;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.Interfaces
{
     public interface IUserRepository
     {
          User FindById(string id);
          IEnumerable<User> GetAll();
          void Save(User user);
     }
}
