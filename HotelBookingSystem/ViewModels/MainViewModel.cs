using System.Windows.Input;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Services;

namespace HotelBookingSystem.ViewModels
{
     public class MainViewModel : BaseViewModel
     {
          public GuestController GuestCtrl { get; }
          public RoomController RoomCtrl { get; }
          public BookingController BookingCtrl { get; }
          public LogController LogCtrl { get; }

          public ICommand CreateGuestCommand { get; }
          public ICommand CreateRoomCommand { get; }
          public ICommand CreateBookingCommand { get; }
          public ICommand RefreshBookingsCommand { get; }
          public ICommand ConfirmBookingCommand { get; }
          public ICommand CancelBookingCommand { get; }

          private readonly IRoomPricingService _roomPricingService;

          public MainViewModel()
          {
               ILogger logger = new ConsoleLogger();
               IBookingRepository bookingRepository = new InMemoryBookingRepository();
               IRoomRepository roomRepository = new InMemoryRoomRepository();
               IUserRepository userRepository = new InMemoryUserRepository();
               IUserValidator userValidator = new UserValidator();
               IRoomPricingService roomPricingService = new RoomPricingService();
               IBookingDurationCalculator durationCalculator = new BookingDurationCalculator();
               IBookingConfirmationService confirmationService = new BookingConfirmationService();
               BookingFactoryProvider bookingFactory = new BookingFactoryProvider();

               IBookingService bookingService = new BookingService(
                   bookingRepository, roomRepository, userRepository,
                   confirmationService, userValidator, logger);

               _roomPricingService = roomPricingService;

               LogCtrl = new LogController(logger);
               GuestCtrl = new GuestController(userRepository, userValidator);
               RoomCtrl = new RoomController(roomRepository, roomPricingService);
               BookingCtrl = new BookingController(bookingService, bookingRepository, durationCalculator, bookingFactory);

               GuestCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               RoomCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               BookingCtrl.OnLog += msg => LogCtrl.AddLog(msg);

               CreateGuestCommand = new RelayCommand(_ => GuestCtrl.CreateGuest());
               CreateRoomCommand = new RelayCommand(_ => RoomCtrl.CreateRoom());
               CreateBookingCommand = new RelayCommand(_ => BookingCtrl.CreateBooking(GuestCtrl.CurrentUser, RoomCtrl.CurrentRoom, _roomPricingService));
               RefreshBookingsCommand = new RelayCommand(_ => BookingCtrl.RefreshBookings());
               ConfirmBookingCommand = new RelayCommand(_ => BookingCtrl.ConfirmBooking(), _ => BookingCtrl.SelectedBooking != null);
               CancelBookingCommand = new RelayCommand(_ => BookingCtrl.CancelBooking(), _ => BookingCtrl.SelectedBooking != null);

               BookingCtrl.RefreshBookings();
          }
     }
}