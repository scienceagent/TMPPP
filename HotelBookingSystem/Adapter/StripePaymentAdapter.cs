namespace HotelBookingSystem.Adapter
{
     public class StripePaymentAdapter : IPaymentService
     {
          private readonly StripePaymentGateway _stripe;
          private string _lastTransactionId = string.Empty;

          public StripePaymentAdapter(StripePaymentGateway stripe) => _stripe = stripe;

          public bool ProcessPayment(string guestId, decimal amount)
          {
               bool success = _stripe.ChargeCard(guestId, (double)(amount * 100), "USD");
               if (success) _lastTransactionId = _stripe.GetLastChargeId();
               return success;
          }

          public bool RefundPayment(string guestId, decimal amount)
              => _stripe.RefundCharge(_lastTransactionId, (double)(amount * 100));

          public string GetLastTransactionId() => _lastTransactionId;
     }
}