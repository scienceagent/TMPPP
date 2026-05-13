using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelBookingSystem.State;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models.User;
using System.Linq;

namespace HotelBookingSystem.ViewModels
{
    public class StateController : BaseViewModel
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        
        private Booking? _activeBooking;
        private string _currentStateName = "No Selection";
        private string _currentStateDescription = "Select a booking to manage its lifecycle.";
        private string _currentStateColor = "#64748B";
        private string _errorMessage = "";
        private string _guestName = "-";

        public StateController(IBookingRepository bookingRepository, IUserRepository userRepository)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            
            ConfirmCommand = new RelayCommand(_ => TryAction(() => _activeBooking?.Confirm()));
            CheckInCommand = new RelayCommand(_ => TryAction(() => _activeBooking?.CheckIn()));
            CheckOutCommand = new RelayCommand(_ => TryAction(() => _activeBooking?.Complete()));
            CancelCommand = new RelayCommand(_ => TryAction(() => _activeBooking?.Cancel()));
            RefreshCommand = new RelayCommand(_ => LoadData());

            LoadData();
        }

        // --- Data Binding Properties ---
        public ObservableCollection<Booking> AllBookings { get; } = new ObservableCollection<Booking>();
        
        public Booking? SelectedBooking
        {
            get => _activeBooking;
            set
            {
                if (SetProperty(ref _activeBooking, value))
                {
                    UpdateProperties();
                    Log($"Switched to booking: {value?.BookingId ?? "None"}");
                }
            }
        }

        public string BookingInfo => _activeBooking != null 
            ? $"ID: {_activeBooking.BookingId} | Guest: {GuestName}" 
            : "Select a booking from the dropdown.";

        public string GuestName
        {
            get => _guestName;
            private set => SetProperty(ref _guestName, value);
        }

        public string CurrentStateName => _currentStateName;
        public string CurrentStateDescription => _currentStateDescription;
        public string CurrentStateColor => _currentStateColor;
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<string> History { get; } = new ObservableCollection<string>();

        // --- Commands ---
        public ICommand ConfirmCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand CheckOutCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RefreshCommand { get; }

        private void TryAction(Action action)
        {
            if (_activeBooking == null) return;
            
            try
            {
                ErrorMessage = "";
                string oldState = _activeBooking.CurrentState.StateName;
                action();
                
                // Persistence
                _bookingRepository.Save(_activeBooking);
                
                Log($"Success: {oldState} -> {_activeBooking.CurrentState.StateName}");
                UpdateProperties();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                Log($"Error: {ex.Message}");
            }
        }

        private void LoadData()
        {
            var bookings = _bookingRepository.GetAllBookings();
            AllBookings.Clear();
            foreach (var b in bookings)
            {
                AllBookings.Add(b);
            }

            if (SelectedBooking == null && AllBookings.Any())
            {
                SelectedBooking = AllBookings.Last();
            }
        }

        private void UpdateProperties()
        {
            if (_activeBooking == null)
            {
                _currentStateName = "No Selection";
                _currentStateDescription = "Please select a booking.";
                _currentStateColor = "#64748B";
                GuestName = "-";
            }
            else
            {
                _currentStateName = _activeBooking.CurrentState.StateName;
                _currentStateDescription = _activeBooking.CurrentState.Description;
                _currentStateColor = _activeBooking.CurrentState.StatusColor;

                var user = _userRepository.FindById(_activeBooking.UserId);
                GuestName = user?.Name ?? _activeBooking.UserId;
            }
            
            OnPropertyChanged(nameof(BookingInfo));
            OnPropertyChanged(nameof(CurrentStateName));
            OnPropertyChanged(nameof(CurrentStateDescription));
            OnPropertyChanged(nameof(CurrentStateColor));
        }

        private void Log(string message)
        {
            History.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
