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
                      $"Total: {totalPrice.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}";
          }

          public string GetConfirmationType() => "Standard";
     }
}