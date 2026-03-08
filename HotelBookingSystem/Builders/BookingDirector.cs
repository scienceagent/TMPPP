using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Builders
{
     public partial class BookingDirector
     {
          private readonly IBookingBuilder _builder;
          
          public BookingDirector(IBookingBuilder builder)
              => _builder = builder;
          // constructia bookingurilor standard, premium si VIP folosind aceleasi date de intrare, dar cu configurari diferite pentru fiecare tip
          public BookingRequest BuildStandard(string guestId, string roomId,
              DateTime checkIn, DateTime checkOut) =>
              _builder
                  .SetGuest(guestId)
                  .SetRoom(roomId)
                  .SetDates(checkIn, checkOut)
                  .SetBookingType("Standard")
                  .GetResult();

          public BookingRequest BuildPremium(string guestId, string roomId,
              DateTime checkIn, DateTime checkOut) =>
              _builder
                  .SetGuest(guestId)
                  .SetRoom(roomId)
                  .SetDates(checkIn, checkOut)
                  .SetBookingType("Premium")
                  .WithBreakfast()
                  .GetResult();

          public BookingRequest BuildVip(string guestId, string roomId,
              DateTime checkIn, DateTime checkOut) =>
              _builder
                  .SetGuest(guestId)
                  .SetRoom(roomId)
                  .SetDates(checkIn, checkOut)
                  .SetBookingType("VIP")
                  .WithBreakfast()
                  .WithAirportTransfer()
                  .WithSpecialRequest("VIP welcome package")
                  .GetResult();
     }
}