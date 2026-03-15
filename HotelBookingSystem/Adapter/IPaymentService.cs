namespace HotelBookingSystem.Adapter
{
     public interface IPaymentService
     {
          bool ProcessPayment(string guestId, decimal amount);
          bool RefundPayment(string guestId, decimal amount);
          string GetLastTransactionId();
     }
}