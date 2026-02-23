using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IBookingDurationCalculator
     {
          int CalculateDuration(DateTime checkIn, DateTime checkOut);
          int CalculateNights(Booking booking);
          bool IsLongStay(Booking booking);
     }
}