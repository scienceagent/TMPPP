using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.Interfaces
{
     public interface IUserValidator
     {
          bool Validate(User user);
     }
}