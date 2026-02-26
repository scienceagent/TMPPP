using System.Collections.Generic;
using System.Globalization;

namespace HotelBookingSystem.Prototype
{
     public class StandardRoomPrototype : IPrototype<StandardRoomPrototype>
     {
          public string RoomNumber { get; set; }
          public decimal BasePrice { get; set; }
          public int Capacity { get; set; }
          public bool IsAvailable { get; set; }

          public StandardRoomPrototype(string roomNumber, decimal basePrice, int capacity)
          {
               RoomNumber = roomNumber;
               BasePrice = basePrice;
               Capacity = capacity;
               IsAvailable = true;
          }

          public StandardRoomPrototype Clone() =>
              new StandardRoomPrototype(RoomNumber, BasePrice, Capacity)
              {
                   IsAvailable = this.IsAvailable
              };

          public string GetDisplayInfo() =>
              $"[Standard] {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | Capacity: {Capacity}";
     }

     public class DeluxeRoomPrototype : IPrototype<DeluxeRoomPrototype>
     {
          public string RoomNumber { get; set; }
          public decimal BasePrice { get; set; }
          public int Capacity { get; set; }
          public bool IsAvailable { get; set; }
          public bool HasBalcony { get; set; }
          public List<string> Amenities { get; set; }

          public DeluxeRoomPrototype(string roomNumber, decimal basePrice, int capacity,
                                     bool hasBalcony, List<string> amenities)
          {
               RoomNumber = roomNumber;
               BasePrice = basePrice;
               Capacity = capacity;
               IsAvailable = true;
               HasBalcony = hasBalcony;
               Amenities = amenities;
          }

          public DeluxeRoomPrototype Clone() =>
              new DeluxeRoomPrototype(RoomNumber, BasePrice, Capacity, HasBalcony,
                                      new List<string>(Amenities))   // deep copy
              {
                   IsAvailable = this.IsAvailable
              };

          public string GetDisplayInfo() =>
              $"[Deluxe] {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | Balcony: {HasBalcony} | Capacity: {Capacity}";
     }

     public class SuitePrototype : IPrototype<SuitePrototype>
     {
          public string RoomNumber { get; set; }
          public decimal BasePrice { get; set; }
          public int Capacity { get; set; }
          public bool IsAvailable { get; set; }
          public bool HasKitchen { get; set; }
          public bool HasLivingRoom { get; set; }
          public int NumberOfRooms { get; set; }

          public SuitePrototype(string roomNumber, decimal basePrice, int capacity,
                                bool hasKitchen, bool hasLivingRoom, int numberOfRooms)
          {
               RoomNumber = roomNumber;
               BasePrice = basePrice;
               Capacity = capacity;
               IsAvailable = true;
               HasKitchen = hasKitchen;
               HasLivingRoom = hasLivingRoom;
               NumberOfRooms = numberOfRooms;
          }

          public SuitePrototype Clone() =>
              new SuitePrototype(RoomNumber, BasePrice, Capacity,
                                 HasKitchen, HasLivingRoom, NumberOfRooms)
              {
                   IsAvailable = this.IsAvailable
              };

          public string GetDisplayInfo() =>
              $"[Suite] {RoomNumber} | {BasePrice.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | {NumberOfRooms} rooms | Capacity: {Capacity}";
     }
}