using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HotelBookingSystem.Builders;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.ViewModels
{
     public class BookingController : BaseViewModel
     {
          private readonly IBookingService _bookingService;
          private readonly IBookingRepository _bookingRepository;
          private readonly IBookingDurationCalculator _durationCalculator;
          private readonly BookingFactoryProvider _factoryProvider;
          private readonly BookingDirector _director;

          private DateTime _checkInDate = DateTime.Today;
          private DateTime _checkOutDate = DateTime.Today.AddDays(1);
          private Booking? _selectedBooking;
          private string _selectedBookingType = "Standard";

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

          public int Nights => Math.Max(0, (CheckOutDate - CheckInDate).Days);
          public string SelectedBookingType { get => _selectedBookingType; set => SetProperty(ref _selectedBookingType, value); }
          public Booking? SelectedBooking { get => _selectedBooking; set => SetProperty(ref _selectedBooking, value); }

          public ObservableCollection<Booking> Bookings { get; } = new();
          public List<string> BookingTypes { get; }

          public event Action<string>? OnLog;

          public BookingController(
              IBookingService bookingService,
              IBookingRepository bookingRepository,
              IBookingDurationCalculator durationCalculator,
              BookingFactoryProvider factoryProvider,
              BookingDirector director)
          {
               _bookingService = bookingService;
               _bookingRepository = bookingRepository;
               _durationCalculator = durationCalculator;
               _factoryProvider = factoryProvider;
               _director = director;
               BookingTypes = new List<string>(_factoryProvider.GetAvailableTypes());
          }

          public void CreateBooking(User? user, Room? room, IRoomPricingService pricingService)
          {
               if (user == null)
               {
                    OnLog?.Invoke("Error: Register a guest first.\n");
                    ToastService.Instance.Show("Missing Guest", "Please register a guest first (Step 1).", ToastKind.Error);
                    return;
               }
               if (room == null)
               {
                    OnLog?.Invoke("Error: Assign a room first.\n");
                    ToastService.Instance.Show("Missing Room", "Please assign a room first (Step 2).", ToastKind.Error);
                    return;
               }

               try
               {
                    var request = SelectedBookingType switch
                    {
                         "VIP" => _director.BuildVip(user.Id, room.RoomId, CheckInDate, CheckOutDate),
                         "Premium" => _director.BuildPremium(user.Id, room.RoomId, CheckInDate, CheckOutDate),
                         _ => _director.BuildStandard(user.Id, room.RoomId, CheckInDate, CheckOutDate)
                    };

                    OnLog?.Invoke($"[Builder] {request.BookingType} request built.");
                    OnLog?.Invoke($"  Breakfast: {request.BreakfastIncluded} | Transfer: {request.AirportTransfer}");
                    if (request.SpecialRequest != null)
                         OnLog?.Invoke($"  Note: {request.SpecialRequest}");

                    var factory = _factoryProvider.GetFactory(SelectedBookingType);
                    var booking = factory.CreateBooking(request.BookingId, user.Id, room.RoomId,
                                           CheckInDate, CheckOutDate, SelectedBookingType);
                    var pricing = factory.CreatePricingStrategy();
                    var confirmation = factory.CreateConfirmationHandler();

                    decimal roomPrice = pricingService.CalculatePrice(room);
                    int nights = _durationCalculator.CalculateNights(booking);
                    decimal totalPrice = pricing.CalculateTotalPrice(roomPrice, nights);

                    var result = _bookingService.CreateBooking(booking);

                    if (result.Success)
                    {
                         OnLog?.Invoke($"\n[Abstract Factory] {confirmation.GetConfirmationType()} family used.");
                         OnLog?.Invoke(confirmation.GenerateConfirmation(booking, totalPrice));
                         OnLog?.Invoke($"Pricing: {pricing.GetPricingDescription()}\n");
                         RefreshBookings();

                         ToastService.Instance.Show(
                             "Booking Created",
                             $"{SelectedBookingType} · {nights} night{(nights == 1 ? "" : "s")} · {Usd(totalPrice)}",
                             ToastKind.Success);
                    }
                    else
                    {
                         OnLog?.Invoke($"Booking failed: {result.Message}\n");
                         ToastService.Instance.Show("Booking Failed", result.Message, ToastKind.Error);
                    }
               }
               catch (Exception ex)
               {
                    OnLog?.Invoke($"Error: {ex.Message}\n");
                    ToastService.Instance.Show("Error", ex.Message, ToastKind.Error);
               }
          }

          public void ConfirmBooking()
          {
               if (SelectedBooking == null) return;
               var result = _bookingService.ConfirmBooking(SelectedBooking.BookingId);
               OnLog?.Invoke(result.Success
                   ? $"Confirmed: {SelectedBooking.BookingId}\n"
                   : $"Failed: {result.Message}\n");

               if (result.Success)
                    ToastService.Instance.Show("Booking Confirmed",
                        $"ID {SelectedBooking.BookingId[..8]}… is now Confirmed. Ready for Check-In.",
                        ToastKind.Success);
               else
                    ToastService.Instance.Show("Confirm Failed", result.Message, ToastKind.Error);

               RefreshBookings();
          }

          public void CancelBooking()
          {
               if (SelectedBooking == null) return;
               var result = _bookingService.CancelBooking(SelectedBooking.BookingId);
               OnLog?.Invoke(result.Success
                   ? $"Cancelled: {SelectedBooking.BookingId}\n"
                   : $"Failed: {result.Message}\n");

               if (result.Success)
                    ToastService.Instance.Show("Booking Cancelled",
                        $"ID {SelectedBooking.BookingId[..8]}… has been cancelled.",
                        ToastKind.Warning);
               else
                    ToastService.Instance.Show("Cancel Failed", result.Message, ToastKind.Error);

               RefreshBookings();
          }

          public void RefreshBookings()
          {
               Bookings.Clear();
               foreach (var b in _bookingRepository.GetAllBookings())
                    Bookings.Add(b);
          }

          private static string Usd(decimal v) =>
              v.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
     }
}