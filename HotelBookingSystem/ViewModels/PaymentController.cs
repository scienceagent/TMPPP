using System;
using HotelBookingSystem.Adapter;

namespace HotelBookingSystem.ViewModels
{
     public class PaymentController : BaseViewModel
     {
          private readonly IPaymentService _paymentService;

          private string _paymentStatus = "No transaction yet.";
          private decimal _paymentAmount = 250m;

          public string PaymentStatus
          {
               get => _paymentStatus;
               set => SetProperty(ref _paymentStatus, value);
          }

          public decimal PaymentAmount
          {
               get => _paymentAmount;
               set => SetProperty(ref _paymentAmount, value);
          }

          public event Action<string>? OnLog;

          public PaymentController(IPaymentService paymentService)
          {
               _paymentService = paymentService;
          }

          public void ProcessPayment(string guestId)
          {
               if (string.IsNullOrWhiteSpace(guestId))
               {
                    OnLog?.Invoke("Error: Register a guest first.\n");
                    PaymentStatus = "✗ No guest registered.";
                    return;
               }

               OnLog?.Invoke("[Adapter] StripePaymentAdapter.ProcessPayment() invoked.");
               OnLog?.Invoke($"  Translating: decimal {PaymentAmount} → double {(double)(PaymentAmount * 100)} cents");
               OnLog?.Invoke("  Translating: guestId → cardToken,  adding currencyCode = \"USD\"");

               bool success = _paymentService.ProcessPayment(guestId, PaymentAmount);
               string txId = _paymentService.GetLastTransactionId();

               if (success)
               {
                    PaymentStatus = $"✓ Charged ${PaymentAmount:F2}  |  TX: {txId}";
                    OnLog?.Invoke($"  Result: SUCCESS  |  Transaction ID: {txId}\n");
               }
               else
               {
                    PaymentStatus = "✗ Payment failed.";
                    OnLog?.Invoke("  Result: FAILED\n");
               }
          }

          public void RefundPayment(string guestId)
          {
               if (string.IsNullOrWhiteSpace(guestId))
               {
                    OnLog?.Invoke("Error: No guest to refund.\n");
                    return;
               }

               OnLog?.Invoke("[Adapter] StripePaymentAdapter.RefundPayment() invoked.");
               OnLog?.Invoke($"  Translating: decimal {PaymentAmount} → double {(double)(PaymentAmount * 100)} cents");

               bool success = _paymentService.RefundPayment(guestId, PaymentAmount);
               PaymentStatus = success
                   ? $"✓ Refunded ${PaymentAmount:F2}"
                   : "✗ Refund failed.";

               OnLog?.Invoke($"  Result: {(success ? "SUCCESS" : "FAILED")}\n");
          }
     }
}