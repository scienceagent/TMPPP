using System;
using System.Windows.Input;
using HotelBookingSystem.Adapter;
using HotelBookingSystem.Builders;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Decorator;
using HotelBookingSystem.Facade;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models.User;
using HotelBookingSystem.Prototype;
using HotelBookingSystem.Proxy;
using HotelBookingSystem.Services;
using HotelBookingSystem.Singleton;

namespace HotelBookingSystem.ViewModels
{
     public class MainViewModel : BaseViewModel
     {
          // ── infrastructure ────────────────────────────────────────────────────
          private readonly ILogger _logger;
          private readonly IBookingRepository _bookingRepository;
          private readonly IRoomRepository _realRoomRepository;   // unwrapped
          private readonly IRoomRepository _roomRepository;       // wrapped in caching proxy
          private readonly IUserRepository _userRepository;
          private readonly IBookingService _bookingService;
          private readonly IPaymentService _paymentService;
          private readonly IRoomPricingService _pricingService;
          private readonly HotelFacade _facade;

          // ── decorator notification service (Lab 5) ────────────────────────────
          private IBookingNotificationService _notificationService;
          private readonly System.Collections.Generic.List<string> _notifLog = new();

          // ── controllers ───────────────────────────────────────────────────────
          public GuestController GuestCtrl { get; }
          public RoomController RoomCtrl { get; }
          public BookingController BookingCtrl { get; }
          public PaymentController PaymentCtrl { get; }
          public RoomServiceController ServiceCtrl { get; }
          public FacadeController FacadeCtrl { get; }
          public LogController LogCtrl { get; }

          // Lab 5
          public FlyweightController FlyweightCtrl { get; }
          public DecoratorController DecoratorCtrl { get; }
          public BridgeController BridgeCtrl { get; }
          public ProxyController ProxyCtrl { get; }

          // ── toast ─────────────────────────────────────────────────────────────
          public ToastService Toast => ToastService.Instance;

          // ── singleton display ─────────────────────────────────────────────────
          private string _singletonInfo = "";
          public string SingletonInfo
          {
               get => _singletonInfo;
               set => SetProperty(ref _singletonInfo, value);
          }

          // ── commands ──────────────────────────────────────────────────────────
          public ICommand CreateGuestCommand { get; }
          public ICommand CreateRoomCommand { get; }
          public ICommand CreateBookingCommand { get; }
          public ICommand ConfirmBookingCommand { get; }
          public ICommand CancelBookingCommand { get; }
          public ICommand RefreshBookingsCommand { get; }
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
          public ICommand DismissToastCommand { get; }

          // Lab 5 commands
          public ICommand LoadAmenitiesCommand { get; }
          public ICommand NotifyCreatedCommand { get; }
          public ICommand NotifyConfirmedCommand { get; }
          public ICommand NotifyCancelledCommand { get; }
          public ICommand RefreshDecoratorCommand { get; }
          public ICommand GenerateReportCommand { get; }
          public ICommand TestCacheProxyCommand { get; }
          public ICommand TestAuthProxyCommand { get; }

          public MainViewModel()
          {
               // ── Singleton logger ─────────────────────────────────────────────
               _logger = HotelAuditLogger.Instance;
               var logger2 = HotelAuditLogger.Instance;
               SingletonInfo = $"[Singleton] HotelAuditLogger\nsame instance = {ReferenceEquals(_logger, logger2)}";

               // ── Repositories ─────────────────────────────────────────────────
               _bookingRepository = new InMemoryBookingRepository();
               _realRoomRepository = new InMemoryRoomRepository();
               _userRepository = new InMemoryUserRepository();

               // Lab 5 — wrap real room repository in Caching Proxy
               // All existing code that uses _roomRepository now goes through the proxy transparently
               var proxyLog = new System.Collections.Generic.List<string>();
               _roomRepository = new CachingRoomRepositoryProxy(_realRoomRepository, proxyLog, ttlSeconds: 30);

               // ── Services ─────────────────────────────────────────────────────
               _pricingService = new RoomPricingService();
               var confirmSvc = new BookingConfirmationService();
               var userVal = new UserValidator();

               _bookingService = new BookingService(
                   _bookingRepository, _roomRepository, _userRepository,
                   confirmSvc, userVal, _logger);

               var stripe = new StripePaymentGateway();
               _paymentService = new StripePaymentAdapter(stripe);

               _facade = new HotelFacade(
                   _bookingService, _bookingRepository,
                   _roomRepository, _userRepository,
                   _paymentService, _logger);

               // Lab 5 — Decorator: build default notification chain
               //   LoggingDecorator → EmailDecorator → CoreService
               _notificationService = new LoggingNotificationDecorator(
                   new EmailNotificationDecorator(
                       new BookingNotificationService(), _notifLog),
                   _notifLog);

               // ── Sub-controllers ───────────────────────────────────────────────
               var logCtrl = new LogController(_logger);
               LogCtrl = logCtrl;

               var registry = new RoomPrototypeRegistry();
               GuestCtrl = new GuestController(_userRepository);
               RoomCtrl = new RoomController(_roomRepository, _pricingService, registry);
               BookingCtrl = new BookingController(_bookingService, _bookingRepository,
                                 new BookingDurationCalculator(), new BookingFactoryProvider());
               PaymentCtrl = new PaymentController(_paymentService);
               ServiceCtrl = new RoomServiceController();
               FacadeCtrl = new FacadeController(_facade, _bookingRepository, ServiceCtrl);

               // Lab 5 controllers
               FlyweightCtrl = new FlyweightController(_roomRepository);
               DecoratorCtrl = new DecoratorController(_bookingRepository);
               BridgeCtrl = new BridgeController(_bookingRepository);
               ProxyCtrl = new ProxyController(_realRoomRepository);   // uses unwrapped repo to show proxy behaviour clearly

               // ── Wire log events ──────────────────────────────────────────────
               void Log(string m) => LogCtrl.AddLog(m);

               GuestCtrl.OnLog += Log;
               RoomCtrl.OnLog += Log;
               BookingCtrl.OnLog += Log;
               PaymentCtrl.OnLog += Log;
               ServiceCtrl.OnLog += Log;
               FacadeCtrl.OnLog += Log;

               FlyweightCtrl.OnLog += Log;
               DecoratorCtrl.OnLog += Log;
               BridgeCtrl.OnLog += Log;
               ProxyCtrl.OnLog += Log;

               // ── Commands ─────────────────────────────────────────────────────
               CreateGuestCommand = new RelayCommand(_ => CreateGuest());
               CreateRoomCommand = new RelayCommand(_ => RoomCtrl.CreateRoom());
               CreateBookingCommand = new RelayCommand(_ => CreateBooking());
               ConfirmBookingCommand = new RelayCommand(_ => BookingCtrl.ConfirmBooking());
               CancelBookingCommand = new RelayCommand(_ => BookingCtrl.CancelBooking());
               RefreshBookingsCommand = new RelayCommand(_ => BookingCtrl.RefreshBookings());
               ProcessPaymentCommand = new RelayCommand(_ => PaymentCtrl.ProcessPayment(GuestCtrl.CurrentGuestId));
               RefundPaymentCommand = new RelayCommand(_ => PaymentCtrl.RefundPayment(GuestCtrl.CurrentGuestId));
               AddServiceCommand = new RelayCommand(_ => ServiceCtrl.AddToOrder());
               RemoveServiceCommand = new RelayCommand(_ => ServiceCtrl.RemoveSelected());
               ClearServicesCommand = new RelayCommand(_ => ServiceCtrl.ClearOrder());
               CheckInCommand = new RelayCommand(_ => FacadeCtrl.CheckIn());
               CheckOutCommand = new RelayCommand(_ => FacadeCtrl.CheckOut());
               ShowSummaryCommand = new RelayCommand(_ => FacadeCtrl.ShowSummary());
               RefreshFacadeCommand = new RelayCommand(_ => FacadeCtrl.RefreshBookings());
               ClearLogCommand = new RelayCommand(_ => LogCtrl.ClearLog());
               DismissToastCommand = new RelayCommand(_ => Toast.Dismiss());

               // Lab 5 commands
               LoadAmenitiesCommand = new RelayCommand(_ => FlyweightCtrl.LoadRoomAmenities());
               NotifyCreatedCommand = new RelayCommand(_ => DecoratorCtrl.FireCreated());
               NotifyConfirmedCommand = new RelayCommand(_ => DecoratorCtrl.FireConfirmed());
               NotifyCancelledCommand = new RelayCommand(_ => DecoratorCtrl.FireCancelled());
               RefreshDecoratorCommand = new RelayCommand(_ => DecoratorCtrl.RefreshBookings());
               GenerateReportCommand = new RelayCommand(_ => BridgeCtrl.GenerateReport());
               TestCacheProxyCommand = new RelayCommand(_ => ProxyCtrl.TestCacheProxy());
               TestAuthProxyCommand = new RelayCommand(_ => ProxyCtrl.TestAuthProxy());
          }

          // ── helpers ───────────────────────────────────────────────────────────

          private void CreateGuest()
          {
               GuestCtrl.CreateGuest();
               // Refresh decorator booking list whenever data changes
               DecoratorCtrl.RefreshBookings();
               FacadeCtrl.RefreshBookings();
          }

          private void CreateBooking()
          {
               // Get current guest + room from sub-controllers
               var guestId = GuestCtrl.CurrentGuestId;
               var room = RoomCtrl.CurrentRoom;

               if (string.IsNullOrEmpty(guestId) || room == null)
               {
                    ToastService.Instance.Show(
                        "Missing Data",
                        "Register a guest (Step 1) and assign a room (Step 2) first.",
                        ToastKind.Warning);
                    return;
               }

               BookingCtrl.CreateBooking(guestId, room, _pricingService);

               // Lab 5 Decorator — fire notification for booking created
               var bookings = _bookingRepository.GetAllBookings();
               if (bookings.Count > 0)
               {
                    var newest = bookings[bookings.Count - 1];
                    try { _notificationService.NotifyBookingCreated(newest); }
                    catch { /* notification failures never break booking flow */ }

                    foreach (var line in _notifLog)
                         LogCtrl.AddLog(line);
                    _notifLog.Clear();
               }

               DecoratorCtrl.RefreshBookings();
               FacadeCtrl.RefreshBookings();
          }
     }
}