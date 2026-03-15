using System;
using System.Collections.ObjectModel;
using System.Linq;
using HotelBookingSystem.Facade;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.ViewModels
{
     public class FacadeController : BaseViewModel
     {
          private readonly HotelFacade _facade;
          private readonly IBookingRepository _bookingRepository;
          private readonly RoomServiceController _serviceCtrl;

          private string? _selectedBookingId;
          private string _facadeResult = "Select a booking and use Check In / Check Out.";

          public ObservableCollection<string> BookingIds { get; } = new();

          public string? SelectedBookingId
          {
               get => _selectedBookingId;
               set => SetProperty(ref _selectedBookingId, value);
          }

          public string FacadeResult
          {
               get => _facadeResult;
               set => SetProperty(ref _facadeResult, value);
          }

          public event Action<string>? OnLog;

          public FacadeController(
              HotelFacade facade,
              IBookingRepository bookingRepository,
              RoomServiceController serviceCtrl)
          {
               _facade = facade;
               _bookingRepository = bookingRepository;
               _serviceCtrl = serviceCtrl;
          }

          public void RefreshBookings()
          {
               BookingIds.Clear();
               foreach (var b in _bookingRepository.GetAllBookings())
                    BookingIds.Add(b.BookingId);

               if (BookingIds.Count > 0 && string.IsNullOrEmpty(SelectedBookingId))
                    SelectedBookingId = BookingIds[0];
          }

          public void CheckIn()
          {
               if (string.IsNullOrEmpty(SelectedBookingId))
               {
                    OnLog?.Invoke("Error: Select a booking first.\n");
                    FacadeResult = "✗ No booking selected.";
                    return;
               }

               OnLog?.Invoke($"[Facade] CheckInGuest({SelectedBookingId[..8]}...) called.");
               OnLog?.Invoke("  Internally orchestrates:");
               OnLog?.Invoke("    1. Validate booking status");
               OnLog?.Invoke("    2. Find room + guest");
               OnLog?.Invoke("    3. ProcessPayment  →  IPaymentService  →  [Adapter]  →  StripePaymentGateway");
               OnLog?.Invoke("    4. Log via HotelAuditLogger  →  [Singleton]");

               var result = _facade.CheckInGuest(SelectedBookingId);

               if (result.Success)
               {
                    FacadeResult =
                        $"✓ Check-In Successful\n\n" +
                        $"  Guest      :  {result.GuestName}\n" +
                        $"  Room       :  {result.RoomNumber}\n" +
                        $"  Charged    :  ${result.AmountCharged:F2}\n" +
                        $"  Transaction:  {result.TransactionId}";

                    OnLog?.Invoke($"  ✓ {result.GuestName} → Room {result.RoomNumber}, charged ${result.AmountCharged:F2}\n");
               }
               else
               {
                    FacadeResult = $"✗ Check-In Failed\n\n  Reason: {result.Message}";
                    OnLog?.Invoke($"  ✗ Failed: {result.Message}\n");
               }
          }

          public void CheckOut()
          {
               if (string.IsNullOrEmpty(SelectedBookingId))
               {
                    OnLog?.Invoke("Error: Select a booking first.\n");
                    FacadeResult = "✗ No booking selected.";
                    return;
               }

               var services = _serviceCtrl.OrderedItems.ToList();

               OnLog?.Invoke($"[Facade] CheckOutGuest({SelectedBookingId[..8]}...) called.");
               OnLog?.Invoke($"  Services ordered: {services.Count} item(s)  →  [Composite] GetPrice() called on each");
               OnLog?.Invoke("  Internally orchestrates:");
               OnLog?.Invoke("    1. Sum service totals via RoomServiceComponent.GetPrice()  →  [Composite]");
               OnLog?.Invoke("    2. Charge services  →  IPaymentService  →  [Adapter]");
               OnLog?.Invoke("    3. Release room (CancelBooking)");
               OnLog?.Invoke("    4. Log via HotelAuditLogger  →  [Singleton]");

               var result = _facade.CheckOutGuest(SelectedBookingId, services);

               if (result.Success)
               {
                    var lines = result.ServiceLines.Count > 0
                        ? string.Join("\n", result.ServiceLines)
                        : "  (none)";

                    FacadeResult =
                        $"✓ Check-Out Successful\n\n" +
                        $"  Guest          :  {result.GuestName}\n" +
                        $"  Room           :  {result.RoomNumber}\n" +
                        $"  Services total :  ${result.ServicesTotal:F2}\n\n" +
                        $"  Services:\n{lines}";

                    OnLog?.Invoke($"  ✓ {result.GuestName} checked out. Services total: ${result.ServicesTotal:F2}\n");
               }
               else
               {
                    FacadeResult = $"✗ Check-Out Failed\n\n  Reason: {result.Message}";
                    OnLog?.Invoke($"  ✗ Failed: {result.Message}\n");
               }
          }

          public void ShowSummary()
          {
               if (string.IsNullOrEmpty(SelectedBookingId))
               {
                    FacadeResult = "✗ No booking selected.";
                    return;
               }

               OnLog?.Invoke($"[Facade] GetBookingSummary({SelectedBookingId[..8]}...) called.\n");
               FacadeResult = _facade.GetBookingSummary(SelectedBookingId);
          }
     }
}