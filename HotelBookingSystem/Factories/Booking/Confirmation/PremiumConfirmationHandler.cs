using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Factories.Confirmation
{
     public class PremiumConfirmationHandler : IConfirmationHandler
     {
          public string GenerateConfirmation(Booking booking, decimal totalPrice)
          {
               return $"*** Premium Booking Confirmed ***\n" +
                      $"Booking ID: {booking.BookingId}\n" +
                      $"Total (10% discount): {totalPrice.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}\n" +
                      $"Benefits: Early check-in, Late check-out";
          }

          public string GetConfirmationType() => "Premium";
     }
}