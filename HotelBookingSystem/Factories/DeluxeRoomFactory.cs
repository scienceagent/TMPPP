using System.Collections.Generic;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories
{
     public class DeluxeRoomFactory : IRoomFactory
     {
          private readonly List<string> _defaultAmenities;
          private readonly bool _hasBalcony;

          public DeluxeRoomFactory()
          {
               _defaultAmenities = new List<string> { "Minibar", "Balcony", "Sea View" };
               _hasBalcony = true;
          }

          public DeluxeRoomFactory(List<string> amenities, bool hasBalcony)
          {
               _defaultAmenities = amenities ?? new List<string>();
               _hasBalcony = hasBalcony;
          }

          public Room CreateRoom(string roomId, string roomNumber, decimal basePrice, int capacity)
          {
               return new DeluxeRoom(roomId, roomNumber, basePrice, capacity, _defaultAmenities, _hasBalcony);
          }
     }
}