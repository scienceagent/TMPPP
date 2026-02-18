using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IUserRepository
     {
          User FindById(string id);
          void Save(User user);
     }
}
