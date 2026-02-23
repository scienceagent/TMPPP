using System.Collections.Generic;

namespace HotelBookingSystem.Models.User
{
     public class Admin : User
     {
          public string Role { get; }
          public string Department { get; }
          public IReadOnlyList<string> Permissions { get; }

          public Admin(string id, string name, string email, string phone,
                       string role, string department, List<string> permissions)
              : base(id, name, email, phone)
          {
               Role = role;
               Department = department;
               Permissions = permissions.AsReadOnly();
          }

          public override string GetDisplayInfo() =>
              $"Admin: {Name} - {Role} ({Department})";
     }
}