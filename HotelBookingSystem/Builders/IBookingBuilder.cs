using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Builders
{
     public interface IBookingBuilder
     {
          IBookingBuilder SetGuest(string guestId);
          IBookingBuilder SetRoom(string roomId);
          IBookingBuilder SetDates(DateTime checkIn, DateTime checkOut);
          IBookingBuilder SetBookingType(string type);
          IBookingBuilder WithBreakfast();
          IBookingBuilder WithAirportTransfer();
          IBookingBuilder WithSpecialRequest(string note);
          BookingRequest GetResult();
     }
}