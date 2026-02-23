using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class BookingConfirmationService : IBookingConfirmationService
     {
          public void ConfirmBooking(Booking booking, Room room)
          {
               if (!room.IsAvailable)
                    throw new InvalidOperationException("Room is not available");

               booking.Confirm();
               room.SetAvailability(false);
          }

          public void CancelBooking(Booking booking, Room room)
          {
               booking.Cancel();
               room.SetAvailability(true);
          }

          public void CompleteBooking(Booking booking, Room room)
          {
               booking.Complete();
               room.SetAvailability(true);
          }
     }
}