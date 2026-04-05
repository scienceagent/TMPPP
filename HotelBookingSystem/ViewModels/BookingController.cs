using System;
using System.Collections.ObjectModel;
using System.Globalization;
using HotelBookingSystem.Builders;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     public class BookingController : BaseViewModel
     {
          private readonly IBookingService _bookingService;
          private readonly IBookingRepository _bookingRepository;
          private readonly IBookingDurationCalculator _durationCalculator;
          private readonly BookingFactoryProvider _factoryProvider;

          private DateTime _checkInDate = DateTime.Today;
          private DateTime _checkOutDate = DateTime.Today.AddDays(1);
          private string _selectedBookingType = "Standard";
          private Booking? _selectedBooking;

          public ObservableCollection<Booking> Bookings { get; } = new();
          public ObservableCollection<string> BookingTypes { get; }

          public DateTime CheckInDate
          {
               get => _checkInDate;
               set { if (SetProperty(ref _checkInDate, value)) OnPropertyChanged(nameof(Nights)); }
          }

          public DateTime CheckOutDate
          {
               get => _checkOutDate;
               set { if (SetProperty(ref _checkOutDate, value)) OnPropertyChanged(nameof(Nights)); }
          }

          public string SelectedBookingType
          {
               get => _selectedBookingType;
               set => SetProperty(ref _selectedBookingType, value);
          }

          public int Nights => CheckOutDate > CheckInDate
              ? (CheckOutDate - CheckInDate).Days : 0;

          public Booking? SelectedBooking
          {
               get => _selectedBooking;
               set => SetProperty(ref _selectedBooking, value);
          }

          public event Action<string>? OnLog;

          public BookingController(
              IBookingService bookingService,
              IBookingRepository bookingRepository,
              IBookingDurationCalculator durationCalculator,
              BookingFactoryProvider factoryProvider)
          {
               _bookingService = bookingService;
               _bookingRepository = bookingRepository;
               _durationCalculator = durationCalculator;
               _factoryProvider = factoryProvider;
               BookingTypes = new ObservableCollection<string>(_factoryProvider.GetAvailableTypes());
               _selectedBookingType = BookingTypes[0];
          }

          public void CreateBooking(string guestId, Room room, IRoomPricingService pricingService)
          {
               try
               {
                    // ── Builder ──────────────────────────────────────────────────
                    var builder = new BookingBuilder();
                    var director = new BookingDirector(builder);

                    BookingRequest request = SelectedBookingType switch
                    {
                         "Premium" => director.BuildPremium(guestId, room.RoomId, CheckInDate, CheckOutDate),
                         "VIP" => director.BuildVip(guestId, room.RoomId, CheckInDate, CheckOutDate),
                         _ => director.BuildStandard(guestId, room.RoomId, CheckInDate, CheckOutDate),
                    };

                    OnLog?.Invoke($"[Builder] Built {SelectedBookingType} BookingRequest: {request.BookingId[..8]}...");
                    OnLog?.Invoke($"  Nights: {request.Nights}, Breakfast: {request.BreakfastIncluded}, Transfer: {request.AirportTransfer}");

                    // ── Abstract Factory ──────────────────────────────────────────
                    var factory = _factoryProvider.GetFactory(SelectedBookingType);
                    var booking = factory.CreateBooking(
                        request.BookingId, request.GuestId, request.RoomId,
                        request.CheckInDate, request.CheckOutDate, request.BookingType);

                    var pricing = factory.CreatePricingStrategy();
                    var confirm = factory.CreateConfirmationHandler();

                    decimal price = pricing.CalculateTotalPrice(room.BasePrice, request.Nights);
                    string conf = confirm.GenerateConfirmation(booking, price);

                    OnLog?.Invoke($"[Abstract Factory] {SelectedBookingType} family:");
                    OnLog?.Invoke($"  {pricing.GetPricingDescription()}");
                    OnLog?.Invoke($"  Total: {price.ToString("C", CultureInfo.GetCultureInfo("en-US"))}");

                    var result = _bookingService.CreateBooking(booking);

                    if (result.Success)
                    {
                         Bookings.Add(booking);
                         OnLog?.Invoke($"  ✓ Booking created: {booking.BookingId[..8]}...\n");
                         ToastService.Instance.Show(
                             "Booking Created",
                             $"{SelectedBookingType} booking for {Nights} night(s). Total: {price:C}",
                             ToastKind.Success);
                    }
                    else
                    {
                         OnLog?.Invoke($"  ✗ {result.Message}\n");
                         ToastService.Instance.Show("Booking Failed", result.Message, ToastKind.Error);
                    }
               }
               catch (Exception ex)
               {
                    OnLog?.Invoke($"Error creating booking: {ex.Message}\n");
                    ToastService.Instance.Show("Error", ex.Message, ToastKind.Error);
               }
          }

          public void ConfirmBooking()
          {
               if (SelectedBooking == null)
               {
                    ToastService.Instance.Show("No Selection", "Select a booking to confirm.", ToastKind.Warning);
                    return;
               }

               var result = _bookingService.ConfirmBooking(SelectedBooking.BookingId);
               OnLog?.Invoke($"[Booking] Confirm {SelectedBooking.BookingId[..8]}...: {result.Message}");
               RefreshBookings();

               if (result.Success)
                    ToastService.Instance.Show("Booking Confirmed", result.Message, ToastKind.Success);
               else
                    ToastService.Instance.Show("Confirm Failed", result.Message, ToastKind.Error);
          }

          public void CancelBooking()
          {
               if (SelectedBooking == null)
               {
                    ToastService.Instance.Show("No Selection", "Select a booking to cancel.", ToastKind.Warning);
                    return;
               }

               var result = _bookingService.CancelBooking(SelectedBooking.BookingId);
               OnLog?.Invoke($"[Booking] Cancel {SelectedBooking.BookingId[..8]}...: {result.Message}");
               RefreshBookings();

               if (result.Success)
                    ToastService.Instance.Show("Booking Cancelled", result.Message, ToastKind.Info);
               else
                    ToastService.Instance.Show("Cancel Failed", result.Message, ToastKind.Error);
          }

          public void RefreshBookings()
          {
               Bookings.Clear();
               foreach (var b in _bookingRepository.GetAllBookings())
                    Bookings.Add(b);
          }
     }
}