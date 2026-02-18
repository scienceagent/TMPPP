using System;
using System.Collections.Generic;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Factories
{
     public class RoomFactoryProvider
     {
          private readonly Dictionary<string, IRoomFactory> _factories;

          public RoomFactoryProvider()
          {
               _factories = new Dictionary<string, IRoomFactory>
               {
                    { "Standard", new StandardRoomFactory() },
                    { "Deluxe", new DeluxeRoomFactory() },
                    { "Suite", new SuiteRoomFactory() }
               };
          }

          public IRoomFactory GetFactory(string roomType)
          {
               if (_factories.TryGetValue(roomType, out var factory))
                    return factory;

               throw new ArgumentException($"Unknown room type: {roomType}");
          }
     }
}