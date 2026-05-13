namespace HotelBookingSystem.Models.User
{
     public class Guest : User
     {
          public string Nationality { get; private set; }
          public string PassportNumber { get; private set; }

          private Guest() : base() { } // For EF Core

          public Guest(string id, string name, string email, string phone, string username, string password,
                       string nationality, string passportNumber)
              : base(id, name, email, phone, username, password)
          {
               Nationality = nationality;
               PassportNumber = passportNumber;
          }

          public override string GetDisplayInfo() =>
              $"Guest: {Name} (Nationality: {Nationality})";
     }
}
     