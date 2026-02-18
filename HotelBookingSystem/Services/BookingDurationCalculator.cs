using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class BookingDurationCalculator : IBookingDurationCalculator
     {
          public int CalculateDuration(DateTime checkIn, DateTime checkOut)
          {
               if (checkOut <= checkIn)
                    throw new ArgumentException("Check-out must be after check-in");

               return (checkOut - checkIn).Days;
          }

          public int CalculateNights(Booking booking) =>
              CalculateDuration(booking.CheckInDate, booking.CheckOutDate);

          public bool IsLongStay(Booking booking) => CalculateNights(booking) >= 7;

          public bool CalculateDuration( DateTime checkOut)
          {
               return false;
          }
     }

}