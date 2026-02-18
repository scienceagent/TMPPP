using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IUserValidator
     {
          bool Validate(User user);
     }
}