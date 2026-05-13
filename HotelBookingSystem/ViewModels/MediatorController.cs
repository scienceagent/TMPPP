using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelBookingSystem.Mediator;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using System.Linq;

namespace HotelBookingSystem.ViewModels
{
    public class MediatorController : BaseViewModel
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IRoomRepository _roomRepo;
        
        private readonly HotelCommunicationsHub _hub;
        private readonly ReceptionComponent _reception;
        private readonly HousekeepingComponent _housekeeping;
        private readonly AccountingComponent _accounting;

        private Booking? _selectedBooking;

        public MediatorController(IBookingRepository bookingRepo, IRoomRepository roomRepo)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;

            // 1. Create Mediator
            _hub = new HotelCommunicationsHub();
            _hub.OnBroadcast += (dept, msg) => AddLog(dept, msg);

            // 2. Create Colleagues and register them with mediator
            _reception = new ReceptionComponent(_hub, _bookingRepo);
            _housekeeping = new HousekeepingComponent(_hub, _roomRepo);
            _accounting = new AccountingComponent(_hub);

            _hub.RegisterComponents(_reception, _housekeeping, _accounting);

            // Commands
            CheckOutCommand = new RelayCommand(_ => ExecuteCheckOut());
            RefreshCommand  = new RelayCommand(_ => LoadData());
            LoadData();
        }

        public ObservableCollection<Booking> AllBookings { get; } = new ObservableCollection<Booking>();
        
        public Booking? SelectedBooking
        {
            get => _selectedBooking;
            set => SetProperty(ref _selectedBooking, value);
        }

        public ObservableCollection<MediatorLogEntry> EventLogs { get; } = new ObservableCollection<MediatorLogEntry>();

        public ICommand CheckOutCommand { get; }
        public ICommand RefreshCommand  { get; }

        private void ExecuteCheckOut()
        {
            if (SelectedBooking == null) return;
            
            AddLog("System", $"User initiated Check-Out via Mediator for {SelectedBooking.BookingId}");
            _reception.CheckOut(SelectedBooking.BookingId);
        }

        private void LoadData()
        {
            var bookings = _bookingRepo.GetAllBookings();
            AllBookings.Clear();
            foreach (var b in bookings) AllBookings.Add(b);
            
            if (AllBookings.Any()) SelectedBooking = AllBookings.First();
        }

        private void AddLog(string department, string message)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                EventLogs.Insert(0, new MediatorLogEntry(
                    department, 
                    message, 
                    GetDeptColor(department), 
                    DateTime.Now.ToString("HH:mm:ss")));
            });
        }

        private string GetDeptColor(string dept) => dept switch
        {
            "Reception" => "#3B82F6",    // Blue
            "Housekeeping" => "#10B981", // Emerald
            "Accounting" => "#F59E0B",   // Amber
            _ => "#64748B"               // Slate
        };
    }

    public record MediatorLogEntry(string Department, string Message, string Color, string Timestamp);
}
