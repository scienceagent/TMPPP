namespace HotelBookingSystem.Composite
{
     public class RoomServiceItem : RoomServiceComponent
     {
          private readonly string _name;
          private readonly decimal _price;
          private readonly string _description;

          public RoomServiceItem(string name, decimal price, string description)
          { _name = name; _price = price; _description = description; }

          public override string Name => _name;
          public override decimal GetPrice() => _price;
          public override string GetDescription() => $"{_name} — ${_price:F2}: {_description}";
     }
}