using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services;

namespace HotelBookingSystem.ViewModels
{
     /// <summary>
     /// Main ViewModel for the Hotel Booking System UI.
     /// </summary>
     public class MainViewModel : BaseViewModel
     {
          private readonly BookingService _bookingService;
          private readonly IUserRepository _userRepository;
          private readonly IRoomRepository _roomRepository;
          private readonly IBookingRepository _bookingRepository;
          private readonly IUserValidator _userValidator;
          private readonly IRoomPricingService _roomPricingService;

          private string _guestName;
          private string _guestEmail;
          private string _guestNationality;
          private string _guestPassport;
          private string _roomNumber;
          private decimal _roomPrice;
          private int _roomCapacity;
          private string _selectedRoomType;
          private string _logOutput;
          private Booking _selectedBooking;

          // Properties for UI binding
          public string GuestName
          {
               get => _guestName;
               set => SetProperty(ref _guestName, value);
          }

          public string GuestEmail
          {
               get => _guestEmail;
               set => SetProperty(ref _guestEmail, value);
          }

          public string GuestNationality
          {
               get => _guestNationality;
               set => SetProperty(ref _guestNationality, value);
          }

          public string GuestPassport
          {
               get => _guestPassport;
               set => SetProperty(ref _guestPassport, value);
          }

          public string RoomNumber
          {
               get => _roomNumber;
               set => SetProperty(ref _roomNumber, value);
          }

          public decimal RoomPrice
          {
               get => _roomPrice;
               set => SetProperty(ref _roomPrice, value);
          }

          public int RoomCapacity
          {
               get => _roomCapacity;
               set => SetProperty(ref _roomCapacity, value);
          }

          public string SelectedRoomType
          {
               get => _selectedRoomType;
               set => SetProperty(ref _selectedRoomType, value);
          }

          public string LogOutput
          {
               get => _logOutput;
               set => SetProperty(ref _logOutput, value);
          }

          public List<string> RoomTypes { get; } = new List<string> { "Standard", "Deluxe", "Suite" };

          public ObservableCollection<Booking> Bookings { get; } = new ObservableCollection<Booking>();
          public Booking SelectedBooking
          {
               get => _selectedBooking;
               set
               {
                    if (SetProperty(ref _selectedBooking, value))
                         CommandManager.InvalidateRequerySuggested();
               }
          }

          // Commands
          public ICommand CreateGuestCommand { get; }
          public ICommand CreateRoomCommand { get; }
          public ICommand CreateBookingCommand { get; }
          public ICommand TestPolymorphismCommand { get; }
          public ICommand RefreshBookingsCommand { get; }
          public ICommand ConfirmBookingCommand { get; }
          public ICommand CancelBookingCommand { get; }

          private User _currentUser;
          private Room _currentRoom;

          public MainViewModel()
          {
               ILogger logger = new ConsoleLogger();
               _bookingRepository = new InMemoryBookingRepository();
               _roomRepository = new InMemoryRoomRepository();
               _userRepository = new InMemoryUserRepository();
               IBookingConfirmationService confirmationService = new BookingConfirmationService();
               _userValidator = new UserValidator();
               _roomPricingService = new RoomPricingService();

               _bookingService = new BookingService(
                   _bookingRepository,
                   _roomRepository,
                   _userRepository,
                   confirmationService,
                   _userValidator,
                   logger
               );

               CreateGuestCommand = new RelayCommand(_ => CreateGuest());
               CreateRoomCommand = new RelayCommand(_ => CreateRoom());
               CreateBookingCommand = new RelayCommand(_ => CreateBooking());
               TestPolymorphismCommand = new RelayCommand(_ => TestPolymorphism());
               RefreshBookingsCommand = new RelayCommand(_ => RefreshBookings());
               ConfirmBookingCommand = new RelayCommand(_ => ConfirmBooking(), _ => SelectedBooking != null);
               CancelBookingCommand = new RelayCommand(_ => CancelBooking(), _ => SelectedBooking != null);

               // Default values
               GuestName = "";
               GuestEmail = "";
               GuestNationality = "";
               GuestPassport = "";
               RoomNumber = "101";
               RoomPrice = 200;
               RoomCapacity = 2;
               SelectedRoomType = "Standard";

               LogOutput = "Hotel Booking System ready.\n" +
                          "Create a guest, then a room, then complete your booking.\n\n";

               RefreshBookings();
          }

          private static string FormatUsd(decimal amount)
          {
               return amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
          }

          private void CreateGuest()
          {
               try
               {
                    _currentUser = new Guest(
                        Guid.NewGuid().ToString(),
                        GuestName,
                        GuestEmail,
                        "0721234567",
                        GuestNationality,
                        GuestPassport
                    );
                    _userRepository.Save(_currentUser);

                    AddLog("Guest created.");
                    AddLog($"Name: {_currentUser.Name} | Email: {_currentUser.Email}");
                    AddLog($"Valid: {_userValidator.Validate(_currentUser)}\n");
               }
               catch (Exception ex)
               {
                    AddLog($"Error: {ex.Message}\n");
               }
          }

          private void CreateRoom()
          {
               try
               {
                    switch (SelectedRoomType)
                    {
                         case "Standard":
                              _currentRoom = new StandardRoom(
                                  Guid.NewGuid().ToString(),
                                  RoomNumber,
                                  RoomPrice,
                                  RoomCapacity
                              );
                              break;

                         case "Deluxe":
                              _currentRoom = new DeluxeRoom(
                                  Guid.NewGuid().ToString(),
                                  RoomNumber,
                                  RoomPrice,
                                  RoomCapacity,
                                  new List<string> { "Minibar", "Balcony", "Sea View" },
                                  true
                              );
                              break;

                         case "Suite":
                              _currentRoom = new Suite(
                                  Guid.NewGuid().ToString(),
                                  RoomNumber,
                                  RoomPrice,
                                  3,
                                  true,
                                  true
                              );
                              break;
                    }
                    _roomRepository.Save(_currentRoom);

                    AddLog($"{SelectedRoomType} room created.");
                    AddLog(_currentRoom.GetDisplayInfo());
                    AddLog(_currentRoom.GetDescription());
                    AddLog($"Price: {FormatUsd(_roomPricingService.CalculatePrice(_currentRoom))}");
                    AddLog($"Cleaning cost: {FormatUsd(_roomPricingService.CalculateCleaningCost(_currentRoom))}");
                    AddLog($"Capacity: {_currentRoom.Capacity} guests\n");
               }
               catch (Exception ex)
               {
                    AddLog($"Error: {ex.Message}\n");
               }
          }

          private void CreateBooking()
          {
               try
               {
                    if (_currentUser == null)
                    {
                         AddLog("Please create a guest first.\n");
                         return;
                    }

                    if (_currentRoom == null)
                    {
                         AddLog("Please create a room first.\n");
                         return;
                    }

                    var booking = new Booking(
                        $"BK{DateTime.Now:yyyyMMddHHmmss}",
                        _currentUser.Id,
                        _currentRoom.RoomId,
                        DateTime.Now.AddDays(1),
                        DateTime.Now.AddDays(4)
                    );

                    var result = _bookingService.CreateBooking(booking);
                    if (result.Success)
                    {
                         AddLog("Booking created.");
                         AddLog($"ID: {booking.BookingId}");
                         AddLog($"Status: {booking.Status}\n");
                         RefreshBookings();
                    }
                    else
                         AddLog($"Booking failed: {result.Message}\n");
               }
               catch (Exception ex)
               {
                    AddLog($"Error: {ex.Message}\n");
               }
          }

          private void TestPolymorphism()
          {
               AddLog("=== Test polymorphism ===\n");

               User guest = new Guest("G1", "Test Guest", "guest@test.com", "0721111111", "US", "US111");
               User admin = new Admin("A1", "Test Admin", "admin@test.com", "0721222222",
                                      "Manager", "IT", new List<string> { "ManageRooms", "ViewBookings", "ManageUsers" });

               AddLog("Guest info:");
               AddLog(guest.GetDisplayInfo());
               AddLog($"Valid: {_userValidator.Validate(guest)}\n");

               AddLog("Admin info:");
               AddLog(admin.GetDisplayInfo());
               AddLog($"Valid: {_userValidator.Validate(admin)}\n");

               Room standard = new StandardRoom("R1", "101", 200m, 2);
               Room deluxe = new DeluxeRoom("R2", "201", 350m, 2, new List<string> { "Minibar" }, true);
               Room suite = new Suite("R3", "301", 500m, 3, true, true);

               AddLog("Standard room:");
               AddLog(standard.GetDisplayInfo());
               AddLog(standard.GetDescription());
               AddLog($"Price: {FormatUsd(_roomPricingService.CalculatePrice(standard))}");
               AddLog($"Cleaning: {FormatUsd(_roomPricingService.CalculateCleaningCost(standard))}\n");

               AddLog("Deluxe room:");
               AddLog(deluxe.GetDisplayInfo());
               AddLog(deluxe.GetDescription());
               AddLog($"Price: {FormatUsd(_roomPricingService.CalculatePrice(deluxe))}");
               AddLog($"Cleaning: {FormatUsd(_roomPricingService.CalculateCleaningCost(deluxe))}\n");

               AddLog("Suite:");
               AddLog(suite.GetDisplayInfo());
               AddLog(suite.GetDescription());
               AddLog($"Price: {FormatUsd(_roomPricingService.CalculatePrice(suite))}");
               AddLog($"Cleaning: {FormatUsd(_roomPricingService.CalculateCleaningCost(suite))}\n");
          }

          private void RefreshBookings()
          {
               Bookings.Clear();
               foreach (var b in _bookingRepository.GetAllBookings())
                    Bookings.Add(b);
          }

          private void ConfirmBooking()
          {
               if (SelectedBooking == null) return;
               var result = _bookingService.ConfirmBooking(SelectedBooking.BookingId);
               AddLog(result.Success
                   ? $"Confirmed: {SelectedBooking.BookingId}\n"
                   : $"Confirm failed: {result.Message}\n");
               RefreshBookings();
          }

          private void CancelBooking()
          {
               if (SelectedBooking == null) return;
               var result = _bookingService.CancelBooking(SelectedBooking.BookingId);
               AddLog(result.Success
                   ? $"Cancelled: {SelectedBooking.BookingId}\n"
                   : $"Cancel failed: {result.Message}\n");
               RefreshBookings();
          }

          private void AddLog(string message)
          {
               LogOutput += $"{message}\n";
          }
     }
}