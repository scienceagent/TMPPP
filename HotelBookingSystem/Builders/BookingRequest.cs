using System;

namespace HotelBookingSystem.Models
{
     public class BookingRequest
     {
          public string BookingId { get; init; }
          public string GuestId { get; init; }
          public string RoomId { get; init; }
          public DateTime CheckInDate { get; init; }
          public DateTime CheckOutDate { get; init; }
          public string BookingType { get; init; }
          public bool BreakfastIncluded { get; init; }
          public bool AirportTransfer { get; init; }
          public string? SpecialRequest { get; init; }

          public int Nights => (CheckOutDate - CheckInDate).Days;
     }
}