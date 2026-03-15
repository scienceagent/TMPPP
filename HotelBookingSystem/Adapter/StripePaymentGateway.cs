namespace HotelBookingSystem.Adapter
{
     public class StripePaymentGateway
     {
          public bool ChargeCard(string cardToken, double amountInCents, string currencyCode)
              => amountInCents > 0;

          public bool RefundCharge(string chargeId, double amountInCents)
              => !string.IsNullOrEmpty(chargeId);

          public string GetLastChargeId() => $"ch_{System.Guid.NewGuid():N}";
     }
}