using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories.Confirmation
{
     public class StandardConfirmationHandler : IConfirmationHandler
     {
          public string GenerateConfirmation(Booking booking, decimal totalPrice)
          {
               return $"Standard Booking Confirmed\n" +
                      $"Booking ID: {booking.BookingId}\n" +
                      $"Total: {totalPrice:C2}";
          }

          public string GetConfirmationType() => "Standard";
     }
}