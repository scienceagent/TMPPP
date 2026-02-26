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
          private Booking _selectedBooking;
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
          public Booking SelectedBooking { get => _selectedBooking; set => SetProperty(ref _selectedBooking, value); }

          public ObservableCollection<Booking> Bookings { get; } = new();
          public List<string> BookingTypes { get; }

          public event Action<string> OnLog;

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

          public void CreateBooking(User user, Room room, IRoomPricingService pricingService)
          {
               if (user == null) { OnLog?.Invoke("Error: Register a guest first.\n"); return; }
               if (room == null) { OnLog?.Invoke("Error: Assign a room first.\n"); return; }

               try
               {
                    // Builder + Director: assemble the BookingRequest with correct extras per type
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

                    // Abstract Factory: create the matching Booking, Pricing, and Confirmation objects
                    var factory = _factoryProvider.GetFactory(SelectedBookingType);
                    var booking = factory.CreateBooking(request.BookingId, user.Id, room.RoomId, CheckInDate, CheckOutDate);
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
                    }
                    else
                    {
                         OnLog?.Invoke($"Booking failed: {result.Message}\n");
                    }
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
               OnLog?.Invoke(result.Success ? $"Confirmed: {SelectedBooking.BookingId}\n"
                                            : $"Failed: {result.Message}\n");
               RefreshBookings();
          }

          public void CancelBooking()
          {
               if (SelectedBooking == null) return;
               var result = _bookingService.CancelBooking(SelectedBooking.BookingId);
               OnLog?.Invoke(result.Success ? $"Cancelled: {SelectedBooking.BookingId}\n"
                                            : $"Failed: {result.Message}\n");
               RefreshBookings();
          }

          public void RefreshBookings()
          {
               Bookings.Clear();
               foreach (var b in _bookingRepository.GetAllBookings())
                    Bookings.Add(b);
          }
     }
}