using System.Collections.Generic;

namespace HotelBookingSystem.Models.User
{
     public class Admin : User
     {
          public string Role { get; private set; }
          public string Department { get; private set; }
          public List<string> Permissions { get; private set; }

          private Admin() : base() { } // For EF Core

          public Admin(string id, string name, string email, string phone, string username, string password,
                       string role, string department, List<string> permissions)
              : base(id, name, email, phone, username, password)
          {
               Role = role;
               Department = department;
               Permissions = permissions ?? new List<string>();
          }

          public override string GetDisplayInfo() =>
              $"Admin: {Name} - {Role} ({Department})";
     }
}