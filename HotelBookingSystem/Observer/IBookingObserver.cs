using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Observer
{
     // --------------------------------------------------------------------------
     // BOOKING EVENT - structura imutabila ce transporta date
     // Impacheteaza rapid cine, cand si cu cine s-a realizat actiunea ce va fi trimisa spre ascultatori.
     // In acest mod, ascultatorii primesc acest "pachet" si intervin, nu asteapta la rand.
     // --------------------------------------------------------------------------
     public sealed record BookingEvent(
         string EventId,
         BookingEventType EventType,
         string BookingId,
         string BookingType,   // Standard / Premium / VIP
         string GuestId,
         string GuestName,
         string RoomId,
         string RoomNumber,
         decimal BasePrice,
         DateTime CheckIn,
         DateTime CheckOut,
         int Nights,
         decimal TotalValue,
         DateTime OccurredAt
     )
     {
          // Factory — creates an event from a live Booking + resolved names
          public static BookingEvent From(BookingEventType type,
                                           Booking booking,
                                           string guestName,
                                           string roomNumber,
                                           decimal basePrice)
          {
               int nights = Math.Max(1, (booking.CheckOutDate - booking.CheckInDate).Days);
               return new BookingEvent(
                   EventId: Guid.NewGuid().ToString("N")[..8],
                   EventType: type,
                   BookingId: booking.BookingId,
                   BookingType: booking.BookingType,
                   GuestId: booking.UserId,
                   GuestName: guestName,
                   RoomId: booking.RoomId,
                   RoomNumber: roomNumber,
                   BasePrice: basePrice,
                   CheckIn: booking.CheckInDate,
                   CheckOut: booking.CheckOutDate,
                   Nights: nights,
                   TotalValue: basePrice * nights,
                   OccurredAt: DateTime.Now);
          }
     }

     // -- Event type enum -------------------------------------------------------
     public enum BookingEventType
     {
          BookingCreated,
          BookingConfirmed,
          BookingCancelled,
          GuestCheckedIn,
          GuestCheckedOut
     }

     // --------------------------------------------------------------------------
     // INTERFATA OBSERVATORILOR - IBookingObserver
     // Orice clasa care implementeaza aceasta interfata devine capabila sa primeasca mesaje "Broadcast" de la monitor.
     // Scopul este in principal interpelarea a 5 observatori independenti printr-o simpla actiune "Update"/"OnBookingEvent".
     // --------------------------------------------------------------------------
     public interface IBookingObserver
     {
          string Name { get; }
          string Description { get; }
          string ColorHex { get; }

          /// <summary>Called by Subject for every booking event — synchronous broadcast.</summary>
          void OnBookingEvent(BookingEvent evt);
     }
}

