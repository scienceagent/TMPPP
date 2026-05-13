using System.Globalization;
using System.ComponentModel.DataAnnotations;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Models
{
     public abstract class Room : IRoomProduct, HotelBookingSystem.Visitor.IVisitable
     {
          public void Accept(HotelBookingSystem.Visitor.IVisitor visitor) => visitor.Visit(this);

          [Key]
          public string RoomId { get; private set; }
          public string RoomNumber { get; private set; }
          public decimal BasePrice { get; private set; }
          public bool IsAvailable { get; protected set; }
          public abstract int Capacity { get; protected set; }

          public string ImagePath { get; protected set; }

          protected Room() { } // EF Core

          protected Room(string roomId, string roomNumber, decimal basePrice)
          {
               RoomId = roomId;
               RoomNumber = roomNumber;
               BasePrice = basePrice;
               IsAvailable = true;
               ImagePath = "/Images/4506458-room-2269591_1920.jpg"; // Default
          }

          public virtual void SetAvailability(bool status) => IsAvailable = status;
          public virtual string GetDisplayInfo() => $"Room {RoomNumber} | Capacity: {Capacity}";
          public virtual string GetDescription() => $"Accommodation in room {RoomNumber}.";
          public virtual string GetPriceSummary(decimal price) =>
              price.ToString("C", CultureInfo.GetCultureInfo("en-US"));
     }
}