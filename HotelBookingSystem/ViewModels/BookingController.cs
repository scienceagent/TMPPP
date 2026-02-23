using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models.User;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     public class BookingController : BaseViewModel
     {
          private readonly IBookingService _bookingService;
          private readonly IBookingRepository _bookingRepository;
          private readonly IBookingDurationCalculator _durationCalculator;
          private readonly BookingFactoryProvider _factoryProvider;

          private Booking _selectedBooking;
          private string _selectedBookingType;
          private DateTime _checkInDate;
          private DateTime _checkOutDate;
          private int _nights;

          public ObservableCollection<Booking> Bookings { get; } = new ObservableCollection<Booking>();
          public List<string> BookingTypes { get; }

          public Booking SelectedBooking
          {
               get => _selectedBooking;
               set => SetProperty(ref _selectedBooking, value);
          }

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

          public int Nights
          {
               get => _nights;
               private set => SetProperty(ref _nights, value);
          }

          public event Action<string> OnLog;

          // Teacher's pattern: client receives the factory provider via constructor (Dependency Injection)
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

               BookingTypes = new List<string>(_factoryProvider.GetAvailableTypes());
               SelectedBookingType = "Standard";
               CheckInDate = DateTime.Today.AddDays(1);
               CheckOutDate = DateTime.Today.AddDays(4);
          }

          public void CreateBooking(User user, Room room, IRoomPricingService pricingService)
          {
               try
               {
                    if (user == null) { OnLog?.Invoke("Please register a guest first.\n"); return; }
                    if (room == null) { OnLog?.Invoke("Please assign a room first.\n"); return; }
                    if (CheckOutDate <= CheckInDate) { OnLog?.Invoke("Check-out must be after check-in.\n"); return; }

                    // Abstract Factory — get the factory for the selected type
                    // Teacher's pattern: factory.CreateButton(), factory.CreateCheckbox()
                    IBookingFactory factory = _factoryProvider.GetFactory(SelectedBookingType ?? "Standard");

                    IPricingStrategy pricing = factory.CreatePricingStrategy();
                    IConfirmationHandler confirmation = factory.CreateConfirmationHandler();
                    Booking booking = factory.CreateBooking(
                        $"BK{DateTime.Now:yyyyMMddHHmmss}",
                        user.Id, room.RoomId, CheckInDate, CheckOutDate);

                    BookingResult result = _bookingService.CreateBooking(booking);
                    if (!result.Success) { OnLog?.Invoke($"Booking failed: {result.Message}\n"); return; }

                    int nights = _durationCalculator.CalculateNights(booking);
                    bool isLongStay = _durationCalculator.IsLongStay(booking);
                    decimal roomPrice = pricingService.CalculatePrice(room);
                    decimal total = pricing.CalculateTotalPrice(roomPrice, nights);

                    OnLog?.Invoke($"[{confirmation.GetConfirmationType()}] Booking created.");
                    OnLog?.Invoke($"ID: {booking.BookingId}");
                    OnLog?.Invoke($"Check-in:  {CheckInDate:dd MMM yyyy}");
                    OnLog?.Invoke($"Check-out: {CheckOutDate:dd MMM yyyy}");
                    OnLog?.Invoke($"Nights: {nights}{(isLongStay ? " (long stay)" : "")}");
                    OnLog?.Invoke($"{pricing.GetPricingDescription()}");
                    OnLog?.Invoke($"Total: {total.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}");
                    OnLog?.Invoke("");
                    OnLog?.Invoke(confirmation.GenerateConfirmation(booking, total));
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
               var result = _bookingService.ConfirmBooking(SelectedBooking.BookingId);
               OnLog?.Invoke(result.Success
                   ? $"Confirmed: {SelectedBooking.BookingId}\n"
                   : $"Confirm failed: {result.Message}\n");
               RefreshBookings();
          }

          public void CancelBooking()
          {
               if (SelectedBooking == null) return;
               var result = _bookingService.CancelBooking(SelectedBooking.BookingId);
               OnLog?.Invoke(result.Success
                   ? $"Cancelled: {SelectedBooking.BookingId}\n"
                   : $"Cancel failed: {result.Message}\n");
               RefreshBookings();
          }

          public void RefreshBookings()
          {
               Bookings.Clear();
               foreach (var b in _bookingRepository.GetAllBookings())
                    Bookings.Add(b);
          }

          private void UpdateNights() =>
              Nights = CheckOutDate > CheckInDate ? (CheckOutDate - CheckInDate).Days : 0;
     }
}