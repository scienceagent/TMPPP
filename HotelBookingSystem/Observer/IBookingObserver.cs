using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Observer
{
     // ══════════════════════════════════════════════════════════════════════════
     // BOOKING EVENT — immutable record passed to every observer on notification.
     // Contains all data an observer could need so none must reach back into
     // other subsystems (prevents cascading dependencies).
     // ══════════════════════════════════════════════════════════════════════════
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

     // ── Event type enum ───────────────────────────────────────────────────────
     public enum BookingEventType
     {
          BookingCreated,
          BookingConfirmed,
          BookingCancelled,
          GuestCheckedIn,
          GuestCheckedOut
     }

     // ══════════════════════════════════════════════════════════════════════════
     // OBSERVER INTERFACE
     // Subject (BookingEventMonitor) depends exclusively on this abstraction.
     // Each concrete observer has ONE responsibility and knows nothing about
     // other observers or the Subject's internal state.
     // ══════════════════════════════════════════════════════════════════════════
     public interface IBookingObserver
     {
          string Name { get; }
          string Description { get; }
          string ColorHex { get; }

          /// <summary>Called by Subject for every booking event — synchronous broadcast.</summary>
          void OnBookingEvent(BookingEvent evt);
     }
}