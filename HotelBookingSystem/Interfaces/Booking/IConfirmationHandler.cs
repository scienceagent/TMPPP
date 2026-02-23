using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IConfirmationHandler
     {
          string GenerateConfirmation(Booking booking, decimal totalPrice);
          string GetConfirmationType();
     }
}