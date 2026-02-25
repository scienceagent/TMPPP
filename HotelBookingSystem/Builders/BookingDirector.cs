using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Builders
{
     // ─── DIRECTOR ────────────────────────────────────────────────────
     // Defines preset construction sequences using the builder.
     // Matches GoF: Director knows the ORDER of steps, not the details.
     // Client passes a builder in — Director drives it.
     public partial class BookingDirector
     {
          private readonly IBookingBuilder _builder;

          public BookingDirector(IBookingBuilder builder)
              => _builder = builder;

          // Standard booking — no extras
          public BookingRequest BuildStandard(string guestId, string roomId,
              DateTime checkIn, DateTime checkOut) =>
              _builder
                  .SetGuest(guestId)
                  .SetRoom(roomId)
                  .SetDates(checkIn, checkOut)
                  .SetBookingType("Standard")
                  .GetResult();

          // Premium booking — breakfast included
          public BookingRequest BuildPremium(string guestId, string roomId,
              DateTime checkIn, DateTime checkOut) =>
              _builder
                  .SetGuest(guestId)
                  .SetRoom(roomId)
                  .SetDates(checkIn, checkOut)
                  .SetBookingType("Premium")
                  .WithBreakfast()
                  .GetResult();

          // VIP full package — all extras
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