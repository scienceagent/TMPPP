namespace HotelBookingSystem.Models
{
     public class BookingResult
     {
          public bool Success { get; }
          public string Message { get; }

          private BookingResult(bool success, string message)
          {
               Success = success;
               Message = message ?? "";
          }

          public static BookingResult Ok(string message) => new BookingResult(true, message);
          public static BookingResult Fail(string message) => new BookingResult(false, message);
     }
}
