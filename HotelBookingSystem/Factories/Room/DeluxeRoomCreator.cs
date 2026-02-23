using System.Collections.Generic;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public sealed class DeluxeRoomCreator : RoomCreator
     {
          private readonly List<string> _amenities;
          private readonly bool _hasBalcony;

          public DeluxeRoomCreator()
          {
               _amenities = new List<string> { "Minibar", "Balcony", "Sea View" };
               _hasBalcony = true;
          }

          public DeluxeRoomCreator(List<string> amenities, bool hasBalcony)
          {
               _amenities = amenities ?? new List<string>();
               _hasBalcony = hasBalcony;
          }

          public override IRoomProduct CreateProduct(string roomId, string roomNumber, decimal basePrice, int capacity)
              => new DeluxeRoom(roomId, roomNumber, basePrice, capacity, _amenities, _hasBalcony);
     }
}