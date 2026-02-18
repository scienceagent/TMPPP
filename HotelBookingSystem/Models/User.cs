namespace HotelBookingSystem.Models
{
     public abstract class User
     {
          public string Id { get; }
          public string Name { get; }
          public string Email { get; }
          public string Phone { get; }

          protected User(string id, string name, string email, string phone)
          {
               Id = id;
               Name = name;
               Email = email;
               Phone = phone;
          }

          public virtual string GetDisplayInfo() => $"{Name} ({Email})";
     }
}