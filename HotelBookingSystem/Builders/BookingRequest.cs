using System;

namespace HotelBookingSystem.Models
{
     // ─── PRODUCT ─────────────────────────────────────────────────────
     // The complex object that BookingBuilder constructs step by step.
     // Has many optional fields — classic "telescoping constructor" problem.
     public class BookingRequest
     {
          public string BookingId { get; init; }
          public string GuestId { get; init; }
          public string RoomId { get; init; }
          public DateTime CheckInDate { get; init; }
          public DateTime CheckOutDate { get; init; }
          public string BookingType { get; init; }   // Standard / Premium / VIP
          public bool BreakfastIncluded { get; init; }   // optional
          public bool AirportTransfer { get; init; }   // optional
          public string? SpecialRequest { get; init; }   // optional

          public int Nights => (CheckOutDate - CheckInDate).Days;

          public override string ToString() =>
              $"[{BookingType}] Booking {BookingId} | Guest: {GuestId} | Room: {RoomId} | " +
              $"{CheckInDate:dd MMM} → {CheckOutDate:dd MMM} ({Nights} nights) | " +
              $"Breakfast: {BreakfastIncluded} | Transfer: {AirportTransfer}" +
              (SpecialRequest != null ? $" | Note: {SpecialRequest}" : "");
     }
}