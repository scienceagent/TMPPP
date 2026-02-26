using System.Windows.Input;
using HotelBookingSystem.Builders;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Prototype;
using HotelBookingSystem.Services;
using HotelBookingSystem.Singleton;

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
               // ?? SINGLETON ????????????????????????????????????????????
               // Both logger1 and logger2 are the SAME instance — proof of Singleton
               ILogger logger1 = HotelAuditLogger.Instance;
               ILogger logger2 = HotelAuditLogger.Instance;
               // ReferenceEquals(logger1, logger2) == true

               // Use the Singleton logger throughout the app
               ILogger logger = HotelAuditLogger.Instance;

               // ?? PROTOTYPE ????????????????????????????????????????????
               // Registry holds pre-configured room templates
               // RoomController clones from registry when creating rooms
               var prototypeRegistry = new RoomPrototypeRegistry();

               // ?? BUILDER ??????????????????????????????????????????????
               // Director uses builder to create preset booking packages
               var bookingDirector = new BookingDirector(new BookingBuilder());

               // ?? Existing services (unchanged) ????????????????????????
               IBookingRepository bookingRepository = new InMemoryBookingRepository();
               IRoomRepository roomRepository = new InMemoryRoomRepository();
               IUserRepository userRepository = new InMemoryUserRepository();
               IUserValidator userValidator = new UserValidator();
               IRoomPricingService roomPricingService = new RoomPricingService();
               IBookingDurationCalculator durationCalc = new BookingDurationCalculator();
               IBookingConfirmationService confirmService = new BookingConfirmationService();
               BookingFactoryProvider bookingFactory = new BookingFactoryProvider();

               IBookingService bookingService = new BookingService(
                   bookingRepository, roomRepository, userRepository,
                   confirmService, userValidator, logger);

               _roomPricingService = roomPricingService;

               // ?? Controllers ??????????????????????????????????????????
               LogCtrl = new LogController(logger);
               GuestCtrl = new GuestController(userRepository, userValidator);
               RoomCtrl = new RoomController(roomRepository, roomPricingService, prototypeRegistry);
               BookingCtrl = new BookingController(
                   bookingService, bookingRepository, durationCalc,
                   bookingFactory, bookingDirector);

               GuestCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               RoomCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               BookingCtrl.OnLog += msg => LogCtrl.AddLog(msg);

               // ?? Commands ?????????????????????????????????????????????
               CreateGuestCommand = new RelayCommand(_ => GuestCtrl.CreateGuest());
               CreateRoomCommand = new RelayCommand(_ => RoomCtrl.CreateRoom());
               CreateBookingCommand = new RelayCommand(_ =>
                   BookingCtrl.CreateBooking(GuestCtrl.CurrentUser, RoomCtrl.CurrentRoom, _roomPricingService));
               RefreshBookingsCommand = new RelayCommand(_ => BookingCtrl.RefreshBookings());
               ConfirmBookingCommand = new RelayCommand(_ => BookingCtrl.ConfirmBooking(),
                   _ => BookingCtrl.SelectedBooking != null);
               CancelBookingCommand = new RelayCommand(_ => BookingCtrl.CancelBooking(),
                   _ => BookingCtrl.SelectedBooking != null);

               BookingCtrl.RefreshBookings();
          }
     }
}