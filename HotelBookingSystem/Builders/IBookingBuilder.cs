using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Builders
{
     // ─── BUILDER INTERFACE ───────────────────────────────────────────
     // Declares all construction steps.
     // Matches GoF: Builder declares steps, ConcreteBuilder implements them.
     public interface IBookingBuilder
     {
          IBookingBuilder SetGuest(string guestId);
          IBookingBuilder SetRoom(string roomId);
          IBookingBuilder SetDates(DateTime checkIn, DateTime checkOut);
          IBookingBuilder SetBookingType(string type);
          IBookingBuilder WithBreakfast();
          IBookingBuilder WithAirportTransfer();
          IBookingBuilder WithSpecialRequest(string note);
          BookingRequest GetResult();   // GoF calls this GetResult() — returns the Product
     }
}