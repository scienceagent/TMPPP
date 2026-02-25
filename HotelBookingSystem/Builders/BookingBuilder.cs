using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Builders
{
     // ─── CONCRETE BUILDER ────────────────────────────────────────────
     // Implements the construction steps and assembles the BookingRequest.
     // Matches GoF: ConcreteBuilder implements IBuilder steps + holds product state.
     public class BookingBuilder : IBookingBuilder
     {
          // Internal state — built up step by step
          private string _guestId = string.Empty;
          private string _roomId = string.Empty;
          private DateTime _checkIn = DateTime.Today;
          private DateTime _checkOut = DateTime.Today.AddDays(1);
          private string _type = "Standard";
          private bool _breakfast = false;
          private bool _transfer = false;
          private string? _note = null;

          public IBookingBuilder SetGuest(string guestId) { _guestId = guestId; return this; }
          public IBookingBuilder SetRoom(string roomId) { _roomId = roomId; return this; }
          public IBookingBuilder SetBookingType(string type) { _type = type; return this; }
          public IBookingBuilder WithBreakfast() { _breakfast = true; return this; }
          public IBookingBuilder WithAirportTransfer() { _transfer = true; return this; }
          public IBookingBuilder WithSpecialRequest(string note) { _note = note; return this; }

          public IBookingBuilder SetDates(DateTime checkIn, DateTime checkOut)
          {
               _checkIn = checkIn;
               _checkOut = checkOut;
               return this;
          }

          // GoF: GetResult() delivers the fully assembled Product
          public BookingRequest GetResult()
          {
               if (string.IsNullOrEmpty(_guestId))
                    throw new InvalidOperationException("Guest is required.");
               if (string.IsNullOrEmpty(_roomId))
                    throw new InvalidOperationException("Room is required.");
               if (_checkOut <= _checkIn)
                    throw new InvalidOperationException("Check-out must be after check-in.");

               return new BookingRequest
               {
                    BookingId = Guid.NewGuid().ToString(),
                    GuestId = _guestId,
                    RoomId = _roomId,
                    CheckInDate = _checkIn,
                    CheckOutDate = _checkOut,
                    BookingType = _type,
                    BreakfastIncluded = _breakfast,
                    AirportTransfer = _transfer,
                    SpecialRequest = _note
               };
          }
     }
}