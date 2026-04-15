using System;
using System.Linq;
using System.Windows.Input;
using HotelBookingSystem.Adapter;
using HotelBookingSystem.Builders;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Config;
using HotelBookingSystem.Decorator;
using HotelBookingSystem.Facade;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Observer;
using HotelBookingSystem.Prototype;
using HotelBookingSystem.Proxy;
using HotelBookingSystem.Services;
using HotelBookingSystem.Singleton;
using HotelBookingSystem.Command;
using HotelBookingSystem.Memento;

namespace HotelBookingSystem.ViewModels
{
     public class MainViewModel : BaseViewModel
     {
          // ── Infrastructure ────────────────────────────────────────────────────
          private readonly ILogger _logger;
          private readonly IBookingRepository _bookingRepository;
          private readonly IRoomRepository _realRoomRepository;
          private readonly IRoomRepository _roomRepository;   // caching proxy
          private readonly IUserRepository _userRepository;
          private readonly IBookingService _bookingService;
          private readonly IPaymentService _paymentService;
          private readonly IRoomPricingService _pricingService;
          private readonly HotelFacade _facade;

          // ── Decorator notification chain ──────────────────────────────────────
          private IBookingNotificationService _notificationService;
          private readonly System.Collections.Generic.List<string> _notifLog = new();

          // ── Controllers ───────────────────────────────────────────────────────
          public GuestController GuestCtrl { get; }
          public RoomController RoomCtrl { get; }
          public BookingController BookingCtrl { get; }
          public PaymentController PaymentCtrl { get; }
          public RoomServiceController ServiceCtrl { get; }
          public FacadeController FacadeCtrl { get; }
          public LogController LogCtrl { get; }
          public FlyweightController FlyweightCtrl { get; }
          public DecoratorController DecoratorCtrl { get; }
          public BridgeController BridgeCtrl { get; }
          public ProxyController ProxyCtrl { get; }
          public StrategyController StrategyCtrl { get; }
          public LoginViewModel LoginCtrl { get; }

          private readonly BookingEventMonitor _bookingMonitor;
          public ObserverController ObserverCtrl { get; }

          private readonly BookingCommandInvoker _commandInvoker;
          private readonly BookingOperationReceiver _commandReceiver;
          public CommandController CommandCtrl { get; }

          public MementoController MementoCtrl { get; }

          // ── Toast ─────────────────────────────────────────────────────────────
          public ToastService Toast => ToastService.Instance;

          // ── Config display strings ────────────────────────────────────────────
          private string _singletonInfo = "";
          public string SingletonInfo
          {
               get => _singletonInfo;
               set => SetProperty(ref _singletonInfo, value);
          }

          public string EmailInfo => $"From: {AppSettings.Instance.GmailDefaults.Email}";
          public string ReportDirInfo => $"Reports → {AppSettings.Instance.ReportSettings.OutputDirectory}";

          // ── Commands ──────────────────────────────────────────────────────────
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
          public ICommand LoadAmenitiesCommand { get; }
          public ICommand NotifyCreatedCommand { get; }
          public ICommand NotifyConfirmedCommand { get; }
          public ICommand NotifyCancelledCommand { get; }
          public ICommand RefreshDecoratorCommand { get; }
          public ICommand GenerateReportCommand { get; }
          public ICommand TestCacheProxyCommand { get; }
          public ICommand TestAuthProxyCommand { get; }
          public ICommand LogoutCommand { get; }

          private bool _isAuthenticated;
          public bool IsAuthenticated
          {
               get => _isAuthenticated;
               set => SetProperty(ref _isAuthenticated, value);
          }

          public MainViewModel()
          {
               // ── Authentication ───────────────────────────────────────────────
               LoginCtrl = new LoginViewModel();
               LoginCtrl.OnLoginSuccess += () => IsAuthenticated = true;

               LogoutCommand = new RelayCommand(_ => {
                    IsAuthenticated = false;
                    LoginCtrl.Clear();
               });

               // ── Singleton ────────────────────────────────────────────────────
               _logger = HotelAuditLogger.Instance;
               SingletonInfo = $"[Singleton] HotelAuditLogger\nsame instance = {ReferenceEquals(_logger, HotelAuditLogger.Instance)}";

               // ── Repositories ─────────────────────────────────────────────────
               _bookingRepository = new InMemoryBookingRepository();
               _realRoomRepository = new InMemoryRoomRepository();
               _userRepository = new InMemoryUserRepository();

               var proxyLog = new System.Collections.Generic.List<string>();
               _roomRepository = new CachingRoomRepositoryProxy(
                   _realRoomRepository, proxyLog, ttlSeconds: 30);

               // ── Services ─────────────────────────────────────────────────────
               _pricingService = new RoomPricingService();
               _bookingService = new BookingService(
                   _bookingRepository, _roomRepository, _userRepository,
                   new BookingConfirmationService(), new UserValidator(), _logger);
               _paymentService = new StripePaymentAdapter(new StripePaymentGateway());
               _facade = new HotelFacade(
                   _bookingService, _bookingRepository, _roomRepository,
                   _userRepository, _paymentService, _logger);

               // ── DECORATOR: real Gmail email chain ─────────────────────────────
               // LoggingDecorator → EmailDecorator (real SMTP) → CoreService
               _notificationService =
                   new LoggingNotificationDecorator(
                       new EmailNotificationDecorator(
                           new BookingNotificationService(),
                           _notifLog,
                           _userRepository),  // <-- PASSED CORRECT DEPENDENCY
                       _notifLog);

               // ── Sub-controllers ───────────────────────────────────────────────
               LogCtrl = new LogController(_logger);
               LogCtrl.AddLog($"[Config] Gmail sender : {AppSettings.Instance.GmailDefaults.Email}");
               LogCtrl.AddLog($"[Config] Report output: {AppSettings.Instance.ReportSettings.OutputDirectory}");

               GuestCtrl = new GuestController(_userRepository);
               RoomCtrl = new RoomController(_roomRepository, _pricingService, new RoomPrototypeRegistry());
               BookingCtrl = new BookingController(_bookingService, _bookingRepository,
                                 new BookingDurationCalculator(), new BookingFactoryProvider());
               PaymentCtrl = new PaymentController(_paymentService);
               ServiceCtrl = new RoomServiceController();
               FacadeCtrl = new FacadeController(_facade, _bookingRepository, ServiceCtrl);

               FlyweightCtrl = new FlyweightController(_roomRepository);
               DecoratorCtrl = new DecoratorController(_bookingRepository, _userRepository);
               BridgeCtrl = new BridgeController(_bookingRepository);
               ProxyCtrl = new ProxyController(_realRoomRepository);
               StrategyCtrl = new StrategyController();

               // ── OBSERVER: Create the subject ─────────────────────────────────────────
               _bookingMonitor = new BookingEventMonitor(
                   _bookingRepository,
                   _roomRepository,
                   _userRepository);

               // Register all 5 concrete observers
               _bookingMonitor.Subscribe(new OccupancyObserver());
               _bookingMonitor.Subscribe(new RevenueObserver());
               _bookingMonitor.Subscribe(new AlertObserver());
               _bookingMonitor.Subscribe(new AuditLogObserver());
               _bookingMonitor.Subscribe(new DashboardObserver());

               // Create the ViewModel that drives the dashboard page
               ObserverCtrl = new ObserverController(_bookingMonitor, _bookingRepository);

               // ── COMMAND: Receiver + Invoker ─────────────────────────────────────────────
               _commandReceiver = new BookingOperationReceiver(
                   _bookingRepository,
                   _roomRepository,          // the REAL repo, not the proxy
                   _bookingService);

               _commandInvoker = new BookingCommandInvoker();
               _commandInvoker.OnLog += Log;

               CommandCtrl = new CommandController(
                   _commandInvoker,
                   _commandReceiver,
                   _bookingRepository,
                   _roomRepository);

               CommandCtrl.OnLog += Log;

               MementoCtrl = new MementoController(
                   _bookingRepository,
                   _roomRepository,
                   _userRepository,
                   _bookingService);

               MementoCtrl.OnLog += Log;
               MementoCtrl.OnFormSubmitted += () =>
               {
                   // Refresh dependent controllers after a booking is created via the form
                   BookingCtrl.RefreshBookings();
                   DecoratorCtrl.RefreshBookings();
                   FacadeCtrl.RefreshBookings();
                   ObserverCtrl?.RefreshAll();
               };

               // ── Log wiring ────────────────────────────────────────────────────
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
               StrategyCtrl.OnLog += Log;
               _bookingMonitor.OnLog += Log;
               ObserverCtrl.OnLog += Log;
               CommandCtrl.OnLog += Log;

               // ── Commands ──────────────────────────────────────────────────────
               CreateGuestCommand = new RelayCommand(_ => CreateGuest());
               CreateRoomCommand = new RelayCommand(_ => RoomCtrl.CreateRoom());
               CreateBookingCommand = new RelayCommand(_ => CreateBooking());
               
               ConfirmBookingCommand = new RelayCommand(_ =>
               {
                    BookingCtrl.ConfirmBooking();
                    var confirmed = _bookingRepository.GetAllBookings()
                        .FirstOrDefault(b => b.Status == Models.BookingStatus.Confirmed);
                    if (confirmed != null)
                    {
                        _bookingMonitor.NotifyBookingConfirmed(confirmed);
                        ObserverCtrl.RefreshAll();
                    }
               });

               CancelBookingCommand = new RelayCommand(_ =>
               {
                    var toCancel = BookingCtrl.SelectedBooking;
                    BookingCtrl.CancelBooking();
                    if (toCancel != null)
                    {
                        _bookingMonitor.NotifyBookingCancelled(toCancel);
                        ObserverCtrl.RefreshAll();
                    }
               });
               
               RefreshBookingsCommand = new RelayCommand(_ => BookingCtrl.RefreshBookings());
               ProcessPaymentCommand = new RelayCommand(_ => PaymentCtrl.ProcessPayment(GuestCtrl.CurrentGuestId));
               RefundPaymentCommand = new RelayCommand(_ => PaymentCtrl.RefundPayment(GuestCtrl.CurrentGuestId));
               AddServiceCommand = new RelayCommand(_ => ServiceCtrl.AddToOrder());
               RemoveServiceCommand = new RelayCommand(_ => ServiceCtrl.RemoveSelected());
               ClearServicesCommand = new RelayCommand(_ => ServiceCtrl.ClearOrder());
               
               CheckInCommand = new RelayCommand(_ =>
               {
                   FacadeCtrl.CheckIn();
                   if (FacadeCtrl.SelectedBookingId != null)
                   {
                       var booking = _bookingRepository.FindById(FacadeCtrl.SelectedBookingId);
                       if (booking != null)
                       {
                           _bookingMonitor.NotifyGuestCheckedIn(booking);
                           ObserverCtrl.RefreshAll();
                       }
                   }
               });

               CheckOutCommand = new RelayCommand(_ =>
               {
                   FacadeCtrl.CheckOut();
                   if (FacadeCtrl.SelectedBookingId != null)
                   {
                       var booking = _bookingRepository.FindById(FacadeCtrl.SelectedBookingId);
                       if (booking != null)
                       {
                           _bookingMonitor.NotifyGuestCheckedOut(booking);
                           ObserverCtrl.RefreshAll();
                       }
                   }
               });

               ShowSummaryCommand = new RelayCommand(_ => FacadeCtrl.ShowSummary());
               RefreshFacadeCommand = new RelayCommand(_ => FacadeCtrl.RefreshBookings());
               ClearLogCommand = new RelayCommand(_ => LogCtrl.ClearLog());
               DismissToastCommand = new RelayCommand(_ => Toast.Dismiss());

               LoadAmenitiesCommand = new RelayCommand(_ => FlyweightCtrl.LoadRoomAmenities());
               NotifyCreatedCommand = new RelayCommand(_ => DecoratorCtrl.FireCreated());
               NotifyConfirmedCommand = new RelayCommand(_ => DecoratorCtrl.FireConfirmed());
               NotifyCancelledCommand = new RelayCommand(_ => DecoratorCtrl.FireCancelled());
               RefreshDecoratorCommand = new RelayCommand(_ => DecoratorCtrl.RefreshBookings());

               // Async command — runs on thread pool, never blocks UI
               GenerateReportCommand = new RelayCommand(
                   async _ => await BridgeCtrl.GenerateReportAsync(),
                   _ => !BridgeCtrl.IsBusy);

               TestCacheProxyCommand = new RelayCommand(_ => ProxyCtrl.TestCacheProxy());
               TestAuthProxyCommand = new RelayCommand(_ => ProxyCtrl.TestAuthProxy());
          }

          // ── Private helpers ───────────────────────────────────────────────────

          private void CreateGuest()
          {
               GuestCtrl.CreateGuest();
               DecoratorCtrl.RefreshBookings();
               FacadeCtrl.RefreshBookings();
          }

          private void CreateBooking()
          {
               var guestId = GuestCtrl.CurrentGuestId;
               var room = RoomCtrl.CurrentRoom;

               if (string.IsNullOrEmpty(guestId) || room == null)
               {
                    ToastService.Instance.Show("Missing Data",
                        "Register a guest (Step 1) and assign a room (Step 2) first.",
                        ToastKind.Warning);
                    return;
               }

               BookingCtrl.CreateBooking(guestId, room, _pricingService);

               // Fire decorator chain — real email sent via Gmail, fire-and-forget
               var bookings = _bookingRepository.GetAllBookings();
               if (bookings.Count > 0)
               {
                    try { _notificationService.NotifyBookingCreated(bookings[bookings.Count - 1]); }
                    catch { /* notification never blocks booking */ }

                    foreach (var line in _notifLog) LogCtrl.AddLog(line);
                    _notifLog.Clear();
               }

               var createdBookings2 = _bookingRepository.GetAllBookings();
               if (createdBookings2.Count > 0)
               {
                   var newest2 = createdBookings2[createdBookings2.Count - 1];
                   _bookingMonitor.NotifyBookingCreated(newest2);
                   ObserverCtrl.RefreshAll();   // refresh dashboard metrics
               }

               DecoratorCtrl.RefreshBookings();
               FacadeCtrl.RefreshBookings();
          }
     }
}