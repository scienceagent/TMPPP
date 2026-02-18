using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories.Confirmation
{
     public class VipConfirmationHandler : IConfirmationHandler
     {
          public string GenerateConfirmation(Booking booking, decimal totalPrice)
          {
               return $"***** VIP Booking Confirmed *****\n" +
                      $"Booking ID: {booking.BookingId}\n" +
                      $"Total (VIP rate): {totalPrice:C2}\n" +
                      $"Benefits: Free upgrade, Spa access, Airport transfer,\n" +
                      $"          Early check-in, Late check-out, Free minibar";
          }

          public string GetConfirmationType() => "VIP";
     }
}