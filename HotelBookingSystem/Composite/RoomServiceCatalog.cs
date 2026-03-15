using System.Collections.Generic;

namespace HotelBookingSystem.Composite
{
     public class RoomServiceCatalog
     {
          private readonly List<RoomServiceComponent> _entries = new();

          public RoomServiceCatalog()
          {
               var breakfast = new RoomServicePackage("Breakfast", discountPercent: 10);
               breakfast.Add(new RoomServiceItem("Continental Breakfast", 18m, "Bread, jam, OJ, coffee"));
               breakfast.Add(new RoomServiceItem("Fresh Fruit Plate", 12m, "Seasonal fruits"));

               var spa = new RoomServicePackage("Spa & Wellness", discountPercent: 15);
               spa.Add(new RoomServiceItem("60-min Massage", 90m, "Full body relaxation"));
               spa.Add(new RoomServiceItem("Aromatherapy Session", 50m, "Essential oils"));

               var dining = new RoomServicePackage("In-Room Dining");
               dining.Add(new RoomServiceItem("Club Sandwich", 22m, "Chicken, bacon, veggies"));
               dining.Add(new RoomServiceItem("Pasta Carbonara", 28m, "Creamy carbonara"));
               dining.Add(new RoomServiceItem("Bottle of Wine", 45m, "House red or white"));

               var vipBundle = new RoomServicePackage("VIP Welcome Bundle", discountPercent: 20);
               vipBundle.Add(breakfast);
               vipBundle.Add(new RoomServiceItem("Welcome Champagne", 60m, "Moët & Chandon"));
               vipBundle.Add(new RoomServiceItem("Fresh Flowers", 35m, "Seasonal arrangement"));

               _entries.Add(breakfast);
               _entries.Add(spa);
               _entries.Add(dining);
               _entries.Add(vipBundle);
               _entries.Add(new RoomServiceItem("Extra Pillow", 0m, "Complimentary"));
               _entries.Add(new RoomServiceItem("Late Checkout", 40m, "Extend to 2PM"));
          }

          public IReadOnlyList<RoomServiceComponent> GetAll() => _entries.AsReadOnly();
     }
}