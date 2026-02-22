using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
          private readonly BookingFactoryProvider _bookingFactoryProvider;

          private Booking _selectedBooking;
          private string _selectedBookingType;
          private DateTime _checkInDate;
          private DateTime _checkOutDate;
          private int _nights;

          // Collection bound to the DataGrid on the Bookings page
          public ObservableCollection<Booking> Bookings { get; } = new ObservableCollection<Booking>();

          // Available booking types fed by the Abstract Factory provider
          public List<string> BookingTypes { get; }

          public Booking SelectedBooking
          {
               get => _selectedBooking;
               set => SetProperty(ref _selectedBooking, value);
          }

          // Selected booking type drives which Abstract Factory is used
          public string SelectedBookingType
          {
               get => _selectedBookingType;
               set => SetProperty(ref _selectedBookingType, value);
          }

          public DateTime CheckInDate
          {
               get => _checkInDate;
               set
               {
                    if (SetProperty(ref _checkInDate, value))
                    {
                         // Auto-push check-out forward if it falls on or before check-in
                         if (_checkOutDate <= value)
                              CheckOutDate = value.AddDays(1);

                         UpdateNights();
                    }
               }
          }

          public DateTime CheckOutDate
          {
               get => _checkOutDate;
               set
               {
                    if (SetProperty(ref _checkOutDate, value))
                         UpdateNights();
               }
          }

          // Computed number of nights displayed in the UI
          public int Nights
          {
               get => _nights;
               private set => SetProperty(ref _nights, value);
          }

          public event Action<string> OnLog;

          public BookingController(
              IBookingService bookingService,
              IBookingRepository bookingRepository,
              IBookingDurationCalculator durationCalculator)
          {
               _bookingService = bookingService;
               _bookingRepository = bookingRepository;
               _durationCalculator = durationCalculator;
               _bookingFactoryProvider = new BookingFactoryProvider();

               // Populate the booking type dropdown from the factory provider
               BookingTypes = new List<string>(_bookingFactoryProvider.GetAvailableTypes());
               SelectedBookingType = "Standard";

               // Default dates: check-in tomorrow, check-out in 3 nights
               CheckInDate = DateTime.Today.AddDays(1);
               CheckOutDate = DateTime.Today.AddDays(4);
          }

          private void UpdateNights()
          {
               Nights = CheckOutDate > CheckInDate
                   ? (CheckOutDate - CheckInDate).Days
                   : 0;
          }

          public void CreateBooking(User user, Room room, IRoomPricingService pricingService)
          {
               try
               {
                    if (user == null)
                    {
                         OnLog?.Invoke("Please register a guest first.\n");
                         return;
                    }

                    if (room == null)
                    {
                         OnLog?.Invoke("Please assign a room first.\n");
                         return;
                    }

                    if (CheckOutDate <= CheckInDate)
                    {
                         OnLog?.Invoke("Check-out date must be after check-in date.\n");
                         return;
                    }

                    // ABSTRACT FACTORY — one factory creates the whole family:
                    // Booking (or subclass) + IPricingStrategy + IConfirmationHandler
                    IBookingFactory factory = _bookingFactoryProvider.GetFactory(SelectedBookingType ?? "Standard");

                    IPricingStrategy pricing = factory.CreatePricingStrategy();
                    IConfirmationHandler confirmation = factory.CreateConfirmationHandler();

                    // CreateBooking returns a typed subclass:
                    //   Standard → Booking
                    //   Premium  → PremiumBooking  (EarlyCheckIn, LateCheckOut)
                    //   VIP      → VipBooking       (Spa, AirportTransfer, Minibar, Upgrade...)
                    Booking booking = factory.CreateBooking(
                        $"BK{DateTime.Now:yyyyMMddHHmmss}",
                        user.Id,
                        room.RoomId,
                        CheckInDate,
                        CheckOutDate
                    );

                    BookingResult result = _bookingService.CreateBooking(booking);
                    if (!result.Success)
                    {
                         OnLog?.Invoke($"Booking failed: {result.Message}\n");
                         return;
                    }

                    // Calculate price using the pricing strategy from the same factory
                    int nightsCount = _durationCalculator.CalculateNights(booking);
                    bool isLongStay = _durationCalculator.IsLongStay(booking);
                    decimal roomPrice = pricingService.CalculatePrice(room);
                    decimal totalPrice = pricing.CalculateTotalPrice(roomPrice, nightsCount);

                    OnLog?.Invoke($"[{confirmation.GetConfirmationType()}] Booking created.");
                    OnLog?.Invoke($"ID: {booking.BookingId}");
                    OnLog?.Invoke($"Type: {booking.GetType().Name}");
                    OnLog?.Invoke($"Check-in:  {CheckInDate:dd MMM yyyy}");
                    OnLog?.Invoke($"Check-out: {CheckOutDate:dd MMM yyyy}");
                    OnLog?.Invoke($"Duration: {nightsCount} nights");
                    OnLog?.Invoke($"Pricing: {pricing.GetPricingDescription()}");
                    OnLog?.Invoke($"Total price: {FormatUsd(totalPrice)}");

                    if (isLongStay)
                         OnLog?.Invoke("** Long stay booking (7+ nights) **");

                    OnLog?.Invoke("");
                    OnLog?.Invoke(confirmation.GenerateConfirmation(booking, totalPrice));
                    OnLog?.Invoke("");

                    RefreshBookings();
               }
               catch (Exception ex)
               {
                    OnLog?.Invoke($"Error: {ex.Message}\n");
               }
          }

          public void ConfirmBooking()
          {
               if (SelectedBooking == null) return;
               BookingResult result = _bookingService.ConfirmBooking(SelectedBooking.BookingId);
               OnLog?.Invoke(result.Success
                   ? $"Confirmed: {SelectedBooking.BookingId}\n"
                   : $"Confirm failed: {result.Message}\n");
               RefreshBookings();
          }

          public void CancelBooking()
          {
               if (SelectedBooking == null) return;
               BookingResult result = _bookingService.CancelBooking(SelectedBooking.BookingId);
               OnLog?.Invoke(result.Success
                   ? $"Cancelled: {SelectedBooking.BookingId}\n"
                   : $"Cancel failed: {result.Message}\n");
               RefreshBookings();
          }

          public void RefreshBookings()
          {
               Bookings.Clear();
               foreach (Booking b in _bookingRepository.GetAllBookings())
                    Bookings.Add(b);
          }

          private static string FormatUsd(decimal amount) =>
              amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
     }
}