namespace HotelBookingSystem.Models.User
{
     public class Guest : User
     {
          public string Nationality { get; }
          public string PassportNumber { get; }

          public Guest(string id, string name, string email, string phone,
                       string nationality, string passportNumber)
              : base(id, name, email, phone)
          {
               Nationality = nationality;
               PassportNumber = passportNumber;
          }

          public override string GetDisplayInfo() =>
              $"Guest: {Name} (Nationality: {Nationality})";
     }
}
     