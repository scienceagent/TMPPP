using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelBookingSystem.Command;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using HotelBookingSystem.Commands;     // RelayCommand

namespace HotelBookingSystem.ViewModels
{
     public sealed class CommandController : BaseViewModel
     {
          // ── Core components ────────────────────────────────────────────────────
          private readonly BookingCommandInvoker _invoker;
          private readonly BookingOperationReceiver _receiver;
          private readonly IBookingRepository _bookingRepo;
          private readonly IRoomRepository _roomRepo;

          // ── Form state ─────────────────────────────────────────────────────────
          private string? _selectedBookingId;
          private string? _selectedRoomId;
          private decimal _newRoomPrice = 150m;
          private string _statusMessage = "Select a booking and use the operation buttons below.";
          private string _transactionResult = "Run a transaction demo to see atomic rollback in action.";

          // ── Observable collections ─────────────────────────────────────────────
          public ObservableCollection<string> BookingIds { get; } = new();
          public ObservableCollection<string> RoomIds { get; } = new();
          public ObservableCollection<CommandHistoryEntry> HistoryRows { get; } = new();

          // ── Properties ────────────────────────────────────────────────────────

          public string? SelectedBookingId
          {
               get => _selectedBookingId;
               set
               {
                    if (SetProperty(ref _selectedBookingId, value))
                         RaiseAllCanExecute();
               }
          }

          public string? SelectedRoomId
          {
               get => _selectedRoomId;
               set => SetProperty(ref _selectedRoomId, value);
          }

          public decimal NewRoomPrice
          {
               get => _newRoomPrice;
               set => SetProperty(ref _newRoomPrice, value);
          }

          public string StatusMessage
          {
               get => _statusMessage;
               set => SetProperty(ref _statusMessage, value);
          }

          public string TransactionResult
          {
               get => _transactionResult;
               set => SetProperty(ref _transactionResult, value);
          }

          public int UndoCount => _invoker.UndoCount;
          public int RedoCount => _invoker.RedoCount;

          // Selected booking info for display
          public string SelectedBookingInfo
          {
               get
               {
                    if (string.IsNullOrEmpty(_selectedBookingId)) return "No booking selected.";
                    var b = _bookingRepo.FindById(_selectedBookingId);
                    if (b == null) return "Booking not found.";
                    var room = _roomRepo.FindById(b.RoomId);
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;
                    return $"{b.BookingType} · Room {room?.RoomNumber ?? "?"} · " +
                           $"{b.CheckInDate:dd MMM} → {b.CheckOutDate:dd MMM} " +
                           $"({nights}n) · Status: {b.Status}";
               }
          }

          // ── Commands ────────────────────────────────────────────────────────────
          public ICommand RefreshCommand { get; }
          public ICommand ConfirmBookingCommand { get; }
          public ICommand CancelBookingCommand { get; }
          public ICommand AdjustPriceCommand { get; }
          public ICommand RebookCommand { get; }
          public ICommand UndoCommand { get; }
          public ICommand RedoCommand { get; }
          public ICommand UndoAllCommand { get; }
          public ICommand DemoTransactionCommand { get; }

          public event Action<string>? OnLog;

          // ── Constructor ────────────────────────────────────────────────────────
          public CommandController(
              BookingCommandInvoker invoker,
              BookingOperationReceiver receiver,
              IBookingRepository bookingRepo,
              IRoomRepository roomRepo)
          {
               _invoker = invoker;
               _receiver = receiver;
               _bookingRepo = bookingRepo;
               _roomRepo = roomRepo;

               _invoker.OnLog += m =>
               {
                    OnLog?.Invoke(m);
                    SyncHistory();
                    RaiseStackCounts();
               };

               RefreshCommand = new RelayCommand(_ => RefreshData());

               ConfirmBookingCommand = new RelayCommand(
                   _ => RunCommand(new ConfirmBookingCommand(_receiver, _selectedBookingId!)),
                   _ => !string.IsNullOrEmpty(_selectedBookingId));

               CancelBookingCommand = new RelayCommand(
                   _ => RunCommand(new CancelBookingCommand(_receiver, _selectedBookingId!)),
                   _ => !string.IsNullOrEmpty(_selectedBookingId));

               AdjustPriceCommand = new RelayCommand(
                   _ => RunAdjustPrice(),
                   _ => !string.IsNullOrEmpty(_selectedRoomId) && _newRoomPrice > 0);

               RebookCommand = new RelayCommand(
                   _ => RunRebook(),
                   _ => !string.IsNullOrEmpty(_selectedBookingId)
                     && !string.IsNullOrEmpty(_selectedRoomId));

               UndoCommand = new RelayCommand(
                   _ => { _invoker.Undo(); RefreshData(); },
                   _ => _invoker.UndoCount > 0);

               RedoCommand = new RelayCommand(
                   _ => { _invoker.Redo(); RefreshData(); },
                   _ => _invoker.RedoCount > 0);

               UndoAllCommand = new RelayCommand(
                   _ => { _invoker.UndoAll(); RefreshData(); },
                   _ => _invoker.UndoCount > 0);

               DemoTransactionCommand = new RelayCommand(_ => RunTransactionDemo());

               RefreshData();
          }

          // ── Data refresh ───────────────────────────────────────────────────────
          public void RefreshData()
          {
               var bookings = _bookingRepo.GetAllBookings();
               var rooms = _roomRepo.GetAllRooms();

               BookingIds.Clear();
               foreach (var b in bookings)
                    BookingIds.Add(b.BookingId);

               RoomIds.Clear();
               foreach (var r in rooms)
                    RoomIds.Add(r.RoomId);

               if (BookingIds.Count > 0 && string.IsNullOrEmpty(_selectedBookingId))
                    SelectedBookingId = BookingIds[0];

               if (RoomIds.Count > 0 && string.IsNullOrEmpty(_selectedRoomId))
                    SelectedRoomId = RoomIds[0];

               OnPropertyChanged(nameof(SelectedBookingInfo));
               SyncHistory();
               RaiseStackCounts();
          }

          // ── Execute helpers ────────────────────────────────────────────────────
          private void RunCommand(IHotelCommand cmd)
          {
               try
               {
                    _invoker.Execute(cmd);
                    StatusMessage = $"✓ {cmd.Description}";
                    RefreshData();
                    ToastService.Instance.Show("Command Executed", cmd.Description, ToastKind.Success);
               }
               catch (Exception ex)
               {
                    StatusMessage = $"✗ {ex.Message}";
                    ToastService.Instance.Show("Command Failed", ex.Message, ToastKind.Error);
               }
          }

          private void RunAdjustPrice()
          {
               if (string.IsNullOrEmpty(_selectedRoomId)) return;
               var cmd = new AdjustRoomPriceCommand(_receiver, _selectedRoomId, _newRoomPrice);
               RunCommand(cmd);
          }

          private void RunRebook()
          {
               if (string.IsNullOrEmpty(_selectedBookingId) ||
                   string.IsNullOrEmpty(_selectedRoomId)) return;

               var original = _bookingRepo.FindById(_selectedBookingId);
               if (original == null) { StatusMessage = "Booking not found."; return; }

               // Create new booking on the selected room, same guest + dates
               var newBooking = new Booking(
                   Guid.NewGuid().ToString(),
                   original.UserId,
                   _selectedRoomId,
                   original.CheckInDate,
                   original.CheckOutDate,
                   original.BookingType);

               var cmd = new RebookGuestCommand(_receiver, _selectedBookingId, newBooking);
               RunCommand(cmd);
          }

          // ── Transaction demo — atomic cancel + rebook, with intentional failure ──
          private void RunTransactionDemo()
          {
               var bookings = _bookingRepo.GetAllBookings();
               var rooms = _roomRepo.GetAllRooms();

               if (bookings.Count == 0 || rooms.Count < 2)
               {
                    TransactionResult =
                        "⚠  Need at least 1 booking and 2 rooms to demo the transaction.\n" +
                        "   Create a booking on the New Booking page first.";
                    return;
               }

               // Demonstrate TWO scenarios:
               // A) Successful transaction
               // B) Failed transaction → automatic rollback

               var booking = bookings[0];
               var room1 = rooms[0];
               var room2 = rooms.Count > 1 ? rooms[1] : rooms[0];

               // ── Scenario A: successful rebook ────────────────────────────────
               var newForA = new Booking(
                   Guid.NewGuid().ToString(),
                   booking.UserId,
                   room2.RoomId,
                   booking.CheckInDate,
                   booking.CheckOutDate,
                   booking.BookingType);

               bool successA = _invoker.ExecuteTransaction(
                   new IHotelCommand[]
                   {
                    new ConfirmBookingCommand(_receiver, booking.BookingId),
                    new CreateBookingCommand(_receiver, newForA),
                   },
                   "Confirm + Create companion booking");

               // ── Scenario B: attempt an invalid price (< 0) → triggers rollback ─
               bool successB = false;
               try
               {
                    successB = _invoker.ExecuteTransaction(
                        new IHotelCommand[]
                        {
                        new AdjustRoomPriceCommand(_receiver, room1.RoomId, 200m),
                        // Simulate a step that throws — invalid booking cancel on empty ID
                        new CancelBookingCommand(_receiver, ""),
                        },
                        "Adjust price + invalid step (rollback demo)");
               }
               catch { /* expected */ }

               TransactionResult =
                   $"Transaction A (Confirm + Create): {(successA ? "✓ COMMITTED" : "✗ ROLLED BACK")}\n" +
                   $"Transaction B (Price + Invalid):   {(successB ? "✓ COMMITTED" : "✗ ROLLED BACK (expected)")}\n\n" +
                   "See Activity Log for full step-by-step trace.";

               RefreshData();
          }

          // ── UI sync helpers ────────────────────────────────────────────────────
          private void SyncHistory()
          {
               HistoryRows.Clear();
               foreach (var entry in _invoker.History)
                    HistoryRows.Add(entry);
          }

          private void RaiseStackCounts()
          {
               OnPropertyChanged(nameof(UndoCount));
               OnPropertyChanged(nameof(RedoCount));
          }

          private void RaiseAllCanExecute()
          {
               OnPropertyChanged(nameof(SelectedBookingInfo));
               System.Windows.Input.CommandManager.InvalidateRequerySuggested();
          }
     }
}