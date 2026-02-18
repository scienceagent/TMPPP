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

               BookingTypes = new List<string>(_bookingFactoryProvider.GetAvailableTypes());
               SelectedBookingType = "Standard";
          }

          public void CreateBooking(User user, Room room, IRoomPricingService pricingService)
          {
               try
               {
                    if (user == null)
                    {
                         OnLog?.Invoke("Please create a guest first.\n");
                         return;
                    }

                    if (room == null)
                    {
                         OnLog?.Invoke("Please create a room first.\n");
                         return;
                    }

                    // ABSTRACT FACTORY — creates a family of related objects
                    IBookingFactory factory = _bookingFactoryProvider.GetFactory(SelectedBookingType);
                    IPricingStrategy pricing = factory.CreatePricingStrategy();
                    IConfirmationHandler confirmation = factory.CreateConfirmationHandler();

                    var booking = factory.CreateBooking(
                        $"BK{DateTime.Now:yyyyMMddHHmmss}",
                        user.Id,
                        room.RoomId,
                        DateTime.Now.AddDays(1),
                        DateTime.Now.AddDays(4)
                    );

                    var result = _bookingService.CreateBooking(booking);
                    if (result.Success)
                    {
                         int nights = _durationCalculator.CalculateNights(booking);
                         bool longStay = _durationCalculator.IsLongStay(booking);
                         decimal roomPrice = pricingService.CalculatePrice(room);
                         decimal totalPrice = pricing.CalculateTotalPrice(roomPrice, nights);

                         OnLog?.Invoke($"[{confirmation.GetConfirmationType()}] Booking created.");
                         OnLog?.Invoke($"ID: {booking.BookingId}");
                         OnLog?.Invoke($"Status: {booking.Status}");
                         OnLog?.Invoke($"Duration: {nights} nights");
                         OnLog?.Invoke($"Pricing: {pricing.GetPricingDescription()}");
                         OnLog?.Invoke($"Total price: {FormatUsd(totalPrice)}");
                         if (longStay)
                              OnLog?.Invoke("** Long stay booking (7+ nights) **");
                         OnLog?.Invoke("");
                         OnLog?.Invoke(confirmation.GenerateConfirmation(booking, totalPrice));
                         OnLog?.Invoke("");
                         RefreshBookings();
                    }
                    else
                         OnLog?.Invoke($"Booking failed: {result.Message}\n");
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

          private static string FormatUsd(decimal amount) =>
              amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
     }
}