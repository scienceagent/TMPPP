using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using HotelBookingSystem.Commands;        // RelayCommand
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Memento;
using HotelBookingSystem.Models;
using HotelBookingSystem.Services;

namespace HotelBookingSystem.ViewModels
{
     public sealed class MementoController : BaseViewModel
     {
          // ── Pattern components ─────────────────────────────────────────────────
          private readonly BookingFormOriginator _originator;
          private readonly BookingFormHistory _caretaker;

          // ── External dependencies ──────────────────────────────────────────────
          private readonly IBookingRepository _bookingRepo;
          private readonly IRoomRepository _roomRepo;
          private readonly IUserRepository _userRepo;
          private readonly IBookingService _bookingService;

          // ── Wizard step ───────────────────────────────────────────────────────
          private int _activeStep;
          public int ActiveStep
          {
               get => _activeStep;
               set
               {
                    if (SetProperty(ref _activeStep, value))
                    {
                         _originator.ActiveStep = value;
                         OnPropertyChanged(nameof(IsStep0));
                         OnPropertyChanged(nameof(IsStep1));
                         OnPropertyChanged(nameof(IsStep2));
                    }
               }
          }
          public bool IsStep0 => _activeStep == 0;
          public bool IsStep1 => _activeStep == 1;
          public bool IsStep2 => _activeStep == 2;

          // ── Originator field proxies (all trigger SaveState on change) ─────────

          public string GuestName
          {
               get => _originator.GuestName;
               set { _originator.GuestName = value; OnPropertyChanged(); NotifyFormProperties(); }
          }
          public string GuestEmail
          {
               get => _originator.GuestEmail;
               set { _originator.GuestEmail = value; OnPropertyChanged(); NotifyFormProperties(); }
          }
          public string GuestNationality
          {
               get => _originator.GuestNationality;
               set { _originator.GuestNationality = value; OnPropertyChanged(); }
          }
          public string GuestPassport
          {
               get => _originator.GuestPassport;
               set { _originator.GuestPassport = value; OnPropertyChanged(); }
          }
          public string RoomNumber
          {
               get => _originator.RoomNumber;
               set { _originator.RoomNumber = value; OnPropertyChanged(); NotifyFormProperties(); }
          }
          public string RoomType
          {
               get => _originator.RoomType;
               set { _originator.RoomType = value; OnPropertyChanged(); }
          }
          public decimal RoomPrice
          {
               get => _originator.RoomPrice;
               set { _originator.RoomPrice = value; OnPropertyChanged(); NotifyFormProperties(); }
          }
          public int RoomCapacity
          {
               get => _originator.RoomCapacity;
               set { _originator.RoomCapacity = value; OnPropertyChanged(); }
          }
          public DateTime CheckIn
          {
               get => _originator.CheckIn;
               set { _originator.CheckIn = value; OnPropertyChanged(); NotifyFormProperties(); }
          }
          public DateTime CheckOut
          {
               get => _originator.CheckOut;
               set { _originator.CheckOut = value; OnPropertyChanged(); NotifyFormProperties(); }
          }
          public string BookingType
          {
               get => _originator.BookingType;
               set { _originator.BookingType = value; OnPropertyChanged(); }
          }
          public bool BreakfastIncluded
          {
               get => _originator.BreakfastIncluded;
               set { _originator.BreakfastIncluded = value; OnPropertyChanged(); }
          }
          public bool AirportTransfer
          {
               get => _originator.AirportTransfer;
               set { _originator.AirportTransfer = value; OnPropertyChanged(); }
          }
          public string SpecialRequest
          {
               get => _originator.SpecialRequest;
               set { _originator.SpecialRequest = value; OnPropertyChanged(); }
          }
          public string Notes
          {
               get => _originator.Notes;
               set { _originator.Notes = value; OnPropertyChanged(); }
          }

          // ── Named checkpoint text box ──────────────────────────────────────────
          private string _checkpointName = "";
          public string CheckpointName
          {
               get => _checkpointName;
               set => SetProperty(ref _checkpointName, value);
          }

          // ── Derived display ────────────────────────────────────────────────────
          public bool IsGuestValid => _originator.IsGuestValid;
          public bool IsRoomValid => _originator.IsRoomValid;
          public bool IsDatesValid => _originator.IsDatesValid;
          public bool IsFormComplete => _originator.IsFormComplete;
          public int Nights => _originator.Nights;

          public int UndoCount => _caretaker.UndoCount;
          public int RedoCount => _caretaker.RedoCount;
          public int NamedCount => _caretaker.NamedCount;

          // ── Status / submit message ────────────────────────────────────────────
          private string _statusMessage = "Fill in the form below. Every change is auto-checkpointed.";
          public string StatusMessage
          {
               get => _statusMessage;
               set => SetProperty(ref _statusMessage, value);
          }

          // ── Observable collections ─────────────────────────────────────────────
          public ObservableCollection<string> BookingTypes { get; } =
              new() { "Standard", "Premium", "VIP" };

          public ObservableCollection<string> RoomTypes { get; } =
              new() { "Standard", "Deluxe", "Suite" };

          public ObservableCollection<string> AvailableRoomNumbers { get; } = new();

          public ObservableCollection<BookingFormSnapshot> CheckpointTimeline { get; } = new();

          public ObservableCollection<NamedCheckpoint> NamedCheckpoints { get; } = new();

          // ── Commands ──────────────────────────────────────────────────────────
          public ICommand UndoCommand { get; }
          public ICommand RedoCommand { get; }
          public ICommand SaveCheckpointCommand { get; }
          public ICommand JumpCheckpointCommand { get; }
          public ICommand ResetFormCommand { get; }
          public ICommand NextStepCommand { get; }
          public ICommand PrevStepCommand { get; }
          public ICommand SubmitCommand { get; }
          public ICommand QuickSaveCommand { get; }

          public event Action<string>? OnLog;
          public event Action? OnFormSubmitted;    // raised when booking is created

          // ── Constructor ────────────────────────────────────────────────────────
          public MementoController(
              IBookingRepository bookingRepo,
              IRoomRepository roomRepo,
              IUserRepository userRepo,
              IBookingService bookingService)
          {
               _bookingRepo = bookingRepo;
               _roomRepo = roomRepo;
               _userRepo = userRepo;
               _bookingService = bookingService;

               _originator = new BookingFormOriginator();
               _caretaker = new BookingFormHistory();

               _caretaker.OnLog += m =>
               {
                    OnLog?.Invoke(m);
                    SyncTimeline();
                    RaiseStackCounts();
               };

               // Save an initial blank snapshot so Undo goes back to "empty"
               _caretaker.SaveState(_originator, "Initial blank form");

               LoadAvailableRooms();

               // ── Commands ──────────────────────────────────────────────────────
               UndoCommand = new RelayCommand(
                   _ => { _caretaker.Undo(_originator); PushOriginatorToUI(); },
                   _ => _caretaker.UndoCount > 0);

               RedoCommand = new RelayCommand(
                   _ => { _caretaker.Redo(_originator); PushOriginatorToUI(); },
                   _ => _caretaker.RedoCount > 0);

               SaveCheckpointCommand = new RelayCommand(_ => SaveNamedCheckpoint());

               JumpCheckpointCommand = new RelayCommand(
                   p => JumpToCheckpoint(p as string ?? ""),
                   _ => _caretaker.NamedCount > 0);

               ResetFormCommand = new RelayCommand(_ => ResetForm());

               QuickSaveCommand = new RelayCommand(_ =>
               {
                    _caretaker.SaveState(_originator);
                    StatusMessage = $"✓ Checkpoint saved at {DateTime.Now:HH:mm:ss}";
               });

               NextStepCommand = new RelayCommand(
                   _ => { _caretaker.SaveState(_originator); ActiveStep = Math.Min(2, _activeStep + 1); },
                   _ => _activeStep < 2);

               PrevStepCommand = new RelayCommand(
                   _ => ActiveStep = Math.Max(0, _activeStep - 1),
                   _ => _activeStep > 0);

               SubmitCommand = new RelayCommand(
                   _ => SubmitForm(),
                   _ => _originator.IsFormComplete);
          }

          // ── Auto-save checkpoint on every meaningful field change ──────────────
          // Called from the property setters of key fields
          private void AutoSave()
          {
               _caretaker.SaveState(_originator);
          }

          // ── Named checkpoint ───────────────────────────────────────────────────
          private void SaveNamedCheckpoint()
          {
               string name = string.IsNullOrWhiteSpace(_checkpointName)
                   ? $"Step {_activeStep + 1} · {DateTime.Now:HH:mm}"
                   : _checkpointName;

               _caretaker.SaveNamedCheckpoint(_originator, name);
               CheckpointName = "";
               SyncNamedCheckpoints();
               StatusMessage = $"✓ Named checkpoint saved: '{name}'";
          }

          // ── Jump to a named checkpoint ─────────────────────────────────────────
          private void JumpToCheckpoint(string name)
          {
               if (_caretaker.JumpToCheckpoint(_originator, name))
               {
                    PushOriginatorToUI();
                    StatusMessage = $"↩ Restored to checkpoint: '{name}'";
               }
          }

          // ── Reset to blank ─────────────────────────────────────────────────────
          private void ResetForm()
          {
               _caretaker.SaveState(_originator, "Before reset");
               _originator.Reset();
               PushOriginatorToUI();
               ActiveStep = 0;
               StatusMessage = "Form reset. Previous state saved for Undo.";
          }

          // ── Submit — creates the actual Booking domain object ─────────────────
          private void SubmitForm()
          {
               if (!_originator.IsFormComplete)
               {
                    StatusMessage = "✗ Form is incomplete — fill all required fields.";
                    return;
               }

               // Find or create user
               string userId = _originator.GuestId;
               if (string.IsNullOrEmpty(userId))
               {
                    var guest = new Models.User.Guest(
                        Guid.NewGuid().ToString(),
                        _originator.GuestName,
                        _originator.GuestEmail,
                        "",
                        _originator.GuestNationality.IfEmpty("Unknown"),
                        _originator.GuestPassport.IfEmpty("UNKNOWN"));
                    _userRepo.Save(guest);
                    userId = guest.Id;
               }

               var booking = new Booking(
                   Guid.NewGuid().ToString(),
                   userId,
                   _originator.RoomId,
                   _originator.CheckIn,
                   _originator.CheckOut,
                   _originator.BookingType);

               var result = _bookingService.CreateBooking(booking);

               if (result.Success)
               {
                    StatusMessage = $"✓ Booking created: {booking.BookingId[..8]}… ({_originator.BookingType})";
                    OnLog?.Invoke($"[Memento] Booking submitted from form snapshot — {_originator.GuestName} · Room {_originator.RoomNumber}");
                    _caretaker.ClearAll();
                    _originator.Reset();
                    PushOriginatorToUI();
                    ActiveStep = 0;
                    OnFormSubmitted?.Invoke();
                    ToastService.Instance.Show("Booking Created",
                        $"{_originator.BookingType} booking for {_originator.GuestName}",
                        ToastKind.Success);
               }
               else
               {
                    StatusMessage = $"✗ {result.Message}";
                    ToastService.Instance.Show("Booking Failed", result.Message, ToastKind.Error);
               }
          }

          // ── Sync UI from originator (called after Undo/Redo/Restore) ──────────
          public void PushOriginatorToUI()
          {
               // Re-raise ALL properties so every bound control updates
               OnPropertyChanged(nameof(GuestName));
               OnPropertyChanged(nameof(GuestEmail));
               OnPropertyChanged(nameof(GuestNationality));
               OnPropertyChanged(nameof(GuestPassport));
               OnPropertyChanged(nameof(RoomNumber));
               OnPropertyChanged(nameof(RoomType));
               OnPropertyChanged(nameof(RoomPrice));
               OnPropertyChanged(nameof(RoomCapacity));
               OnPropertyChanged(nameof(CheckIn));
               OnPropertyChanged(nameof(CheckOut));
               OnPropertyChanged(nameof(BookingType));
               OnPropertyChanged(nameof(BreakfastIncluded));
               OnPropertyChanged(nameof(AirportTransfer));
               OnPropertyChanged(nameof(SpecialRequest));
               OnPropertyChanged(nameof(Notes));
               NotifyFormProperties();
               _activeStep = _originator.ActiveStep;
               OnPropertyChanged(nameof(ActiveStep));
               OnPropertyChanged(nameof(IsStep0));
               OnPropertyChanged(nameof(IsStep1));
               OnPropertyChanged(nameof(IsStep2));
          }

          // ── Load available rooms for the dropdown ─────────────────────────────
          public void LoadAvailableRooms()
          {
               AvailableRoomNumbers.Clear();
               foreach (var r in _roomRepo.GetAvailableRooms())
                    AvailableRoomNumbers.Add(r.RoomNumber);
          }

          // ── When user picks a room number — populate room fields ───────────────
          public void SelectRoomByNumber(string roomNumber)
          {
               var room = _roomRepo.GetAllRooms().FirstOrDefault(r => r.RoomNumber == roomNumber);
               if (room == null) return;

               _originator.RoomId = room.RoomId;
               _originator.RoomNumber = room.RoomNumber;
               _originator.RoomType = room.GetType().Name.Replace("Room", "").Replace("Standard", "Standard");
               _originator.RoomPrice = room.BasePrice;
               _originator.RoomCapacity = room.Capacity;

               PushOriginatorToUI();
               _caretaker.SaveState(_originator, $"Selected Room {roomNumber}");
          }

          // ── Sync observable collections from caretaker ─────────────────────────
          private void SyncTimeline()
          {
               CheckpointTimeline.Clear();
               foreach (var snap in _caretaker.UndoHistory.Take(20))
                    CheckpointTimeline.Add(snap);
          }

          private void SyncNamedCheckpoints()
          {
               NamedCheckpoints.Clear();
               foreach (var cp in _caretaker.NamedCheckpoints)
                    NamedCheckpoints.Add(cp);
          }

          private void NotifyFormProperties()
          {
               OnPropertyChanged(nameof(IsGuestValid));
               OnPropertyChanged(nameof(IsRoomValid));
               OnPropertyChanged(nameof(IsDatesValid));
               OnPropertyChanged(nameof(IsFormComplete));
               OnPropertyChanged(nameof(Nights));
          }

          private void RaiseStackCounts()
          {
               OnPropertyChanged(nameof(UndoCount));
               OnPropertyChanged(nameof(RedoCount));
               OnPropertyChanged(nameof(NamedCount));
               System.Windows.Input.CommandManager.InvalidateRequerySuggested();
          }
     }

     // ── string extension helper ────────────────────────────────────────────────
     file static class StringEx
     {
          public static string IfEmpty(this string s, string fallback)
              => string.IsNullOrWhiteSpace(s) ? fallback : s;
     }
}
