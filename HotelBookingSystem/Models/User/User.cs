namespace HotelBookingSystem.Models.User
{
     public abstract class User : HotelBookingSystem.Visitor.IVisitable
     {
          public void Accept(HotelBookingSystem.Visitor.IVisitor visitor) => visitor.Visit(this);

          public string Id { get; private set; }
          public string Name { get; private set; }
          public string Email { get; private set; }
          public string Phone { get; private set; }
          public string Username { get; private set; }
          public string Password { get; private set; }

          protected User() { } // For EF Core

          protected User(string id, string name, string email, string phone, string username, string password)
          {
               Id = id;
               Name = name;
               Email = email;
               Phone = phone;
               Username = username;
               Password = password;
          }

          public virtual string GetDisplayInfo() => $"{Name} ({Email})";
     }
}