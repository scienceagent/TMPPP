using System.Windows.Input;
using HotelBookingSystem.Adapter;
using HotelBookingSystem.Builders;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Composite;
using HotelBookingSystem.Facade;
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
          public PaymentController PaymentCtrl { get; }
          public RoomServiceController ServiceCtrl { get; }
          public FacadeController FacadeCtrl { get; }

          public ICommand CreateGuestCommand { get; }
          public ICommand CreateRoomCommand { get; }
          public ICommand CreateBookingCommand { get; }
          public ICommand RefreshBookingsCommand { get; }
          public ICommand ConfirmBookingCommand { get; }
          public ICommand CancelBookingCommand { get; }

          public ICommand ProcessPaymentCommand { get; }
          public ICommand RefundPaymentCommand { get; }

          public ICommand AddServiceCommand { get; }
          public ICommand RemoveServiceCommand { get; }
          public ICommand ClearServicesCommand { get; }

          public ICommand CheckInCommand { get; }
          public ICommand CheckOutCommand { get; }
          public ICommand ShowSummaryCommand { get; }
          public ICommand RefreshFacadeCommand { get; }
          public ICommand ClearLogCommand { get; }

          public string SingletonInfo { get; }

          private readonly IRoomPricingService _roomPricingService;

          public MainViewModel()
          {
               var logger = HotelAuditLogger.Instance;
               var logger2 = HotelAuditLogger.Instance;
               SingletonInfo = $"Singleton: same instance = {ReferenceEquals(logger, logger2)}";

               var registry = new RoomPrototypeRegistry();
               var bookingDirector = new BookingDirector(new BookingBuilder());

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

               IPaymentService paymentService = new StripePaymentAdapter(new StripePaymentGateway());

               var hotelFacade = new HotelFacade(
                   bookingService, bookingRepository, roomRepository,
                   userRepository, paymentService, logger);

               LogCtrl = new LogController(logger);
               GuestCtrl = new GuestController(userRepository, userValidator);
               RoomCtrl = new RoomController(roomRepository, roomPricingService, registry);
               BookingCtrl = new BookingController(
                   bookingService, bookingRepository, durationCalc, bookingFactory, bookingDirector);
               PaymentCtrl = new PaymentController(paymentService);
               ServiceCtrl = new RoomServiceController();
               FacadeCtrl = new FacadeController(hotelFacade, bookingRepository, ServiceCtrl);

               GuestCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               RoomCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               BookingCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               PaymentCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               ServiceCtrl.OnLog += msg => LogCtrl.AddLog(msg);
               FacadeCtrl.OnLog += msg => LogCtrl.AddLog(msg);

               LogCtrl.AddLog(SingletonInfo);

               CreateGuestCommand = new RelayCommand(_ => GuestCtrl.CreateGuest());
               CreateRoomCommand = new RelayCommand(_ => RoomCtrl.CreateRoom());
               CreateBookingCommand = new RelayCommand(_ =>
                   BookingCtrl.CreateBooking(GuestCtrl.CurrentUser, RoomCtrl.CurrentRoom, _roomPricingService));
               RefreshBookingsCommand = new RelayCommand(_ =>
               {
                    BookingCtrl.RefreshBookings();
                    FacadeCtrl.RefreshBookings();
               });
               ConfirmBookingCommand = new RelayCommand(
                   _ => BookingCtrl.ConfirmBooking(),
                   _ => BookingCtrl.SelectedBooking != null);
               CancelBookingCommand = new RelayCommand(
                   _ => BookingCtrl.CancelBooking(),
                   _ => BookingCtrl.SelectedBooking != null);

               ProcessPaymentCommand = new RelayCommand(_ =>
                   PaymentCtrl.ProcessPayment(GuestCtrl.CurrentUser?.Id ?? string.Empty));
               RefundPaymentCommand = new RelayCommand(_ =>
                   PaymentCtrl.RefundPayment(GuestCtrl.CurrentUser?.Id ?? string.Empty));

               AddServiceCommand = new RelayCommand(_ => ServiceCtrl.AddToOrder());
               RemoveServiceCommand = new RelayCommand(_ => ServiceCtrl.RemoveSelected());
               ClearServicesCommand = new RelayCommand(_ => ServiceCtrl.ClearOrder());

               CheckInCommand = new RelayCommand(_ => FacadeCtrl.CheckIn());
               CheckOutCommand = new RelayCommand(_ => FacadeCtrl.CheckOut());
               ShowSummaryCommand = new RelayCommand(_ => FacadeCtrl.ShowSummary());
               RefreshFacadeCommand = new RelayCommand(_ => FacadeCtrl.RefreshBookings());
               ClearLogCommand = new RelayCommand(_ => LogCtrl.ClearLog());

               BookingCtrl.RefreshBookings();
               FacadeCtrl.RefreshBookings();
          }
     }
}
