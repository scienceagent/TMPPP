using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Composite
{
     public class RoomServicePackage : RoomServiceComponent
     {
          private readonly string _name;
          private readonly List<RoomServiceComponent> _children = new();
          private readonly decimal _discountPercent;

          public RoomServicePackage(string name, decimal discountPercent = 0)
          { _name = name; _discountPercent = discountPercent; }

          public override string Name => _name;
          public IReadOnlyList<RoomServiceComponent> Children => _children.AsReadOnly();

          public override void Add(RoomServiceComponent c) => _children.Add(c);
          public override void Remove(RoomServiceComponent c) => _children.Remove(c);

          public override decimal GetPrice()
          {
               decimal total = 0;
               foreach (var child in _children) total += child.GetPrice();
               return total * (1 - _discountPercent / 100);
          }

          public override string GetDescription()
          {
               var sb = new StringBuilder();
               sb.AppendLine($"📦 {_name} Package" + (_discountPercent > 0 ? $" ({_discountPercent}% off)" : ""));
               foreach (var child in _children)
                    sb.AppendLine($"   • {child.GetDescription()}");
               sb.AppendLine($"   Total: ${GetPrice():F2}");
               return sb.ToString().TrimEnd();
          }
     }
}