using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Builders
{
     public class BookingBuilder : IBookingBuilder
     {
          // construieste un obiect BookingRequest pas cu pas, oferind metode pentru setarea diferitelor atribute ale rezervarii
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

          // setarea datelor de check-in si check-out pentru booking
          public IBookingBuilder SetDates(DateTime checkIn, DateTime checkOut)
          {
               _checkIn = checkIn;
               _checkOut = checkOut;
               return this;
          }

          // validarea unui booking request si returneaza obiectul construit
          public BookingRequest GetResult()
          {
               if (string.IsNullOrEmpty(_guestId))
                    throw new InvalidOperationException("Guest is required.");
               if (string.IsNullOrEmpty(_roomId))
                    throw new InvalidOperationException("Room is required.");
               if (_checkOut <= _checkIn)
                    throw new InvalidOperationException("Check-out must be after check-in.");
               // crearea unui nou BookingRequest cu valorile setate in builder
               var request = new BookingRequest
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

               Reset();
               return request;
          }
          // reseteaza valorile builderului la starea initiala pentru a putea fi reutilizat
          private void Reset()
          {
               _guestId = string.Empty;
               _roomId = string.Empty;
               _checkIn = DateTime.Today;
               _checkOut = DateTime.Today.AddDays(1);
               _type = "Standard";
               _breakfast = false;
               _transfer = false;
               _note = null;
          }
     }
}