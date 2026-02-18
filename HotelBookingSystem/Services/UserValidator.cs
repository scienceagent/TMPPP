using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class UserValidator : IUserValidator
     {
          public bool Validate(User user)
          {
               if (user == null) return false;
               if (string.IsNullOrEmpty(user.Email)) return false;

               switch (user)
               {
                    case Guest guest:
                         return !string.IsNullOrEmpty(guest.PassportNumber) &&
                                !string.IsNullOrEmpty(guest.Nationality);

                    case Admin admin:
                         return !string.IsNullOrEmpty(admin.Role) &&
                                !string.IsNullOrEmpty(admin.Department) &&
                                admin.Permissions.Count > 0;

                    default:
                         return false;
               }
          }
     }
}
     
