using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelBookingSystem.ChainOfResponsibility;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.ViewModels
{
    public class ChainController : BaseViewModel
    {
        private string _guestId = "U-001"; // Default existing user ID
        private string _roomId = "R-101";   // Default existing room ID
        private DateTime _checkIn = DateTime.Now.AddDays(1);
        private DateTime _checkOut = DateTime.Now.AddDays(3);
        private decimal _expectedPrice = 450;
        
        public event Action<string>? OnLog;
        private ObservableCollection<BookingLogEntry> _logs = new ObservableCollection<BookingLogEntry>();
        private readonly IBookingHandler _pipeline;

        public ChainController(IUserRepository userRepository, IRoomRepository roomRepository)
        {
            // Build the functional validation pipeline using DB repositories
            var pipeline = new GuestValidationHandler(userRepository);
            pipeline
                .SetNext(new RoomAvailabilityHandler(roomRepository))
                .SetNext(new StayDurationHandler())
                .SetNext(new PriceVerificationHandler(roomRepository));

            _pipeline = pipeline;

            ProcessCommand = new RelayCommand(ExecuteProcess);
            ClearLogsCommand = new RelayCommand(_ => _logs.Clear());

            // Initial logs
            _logs.Add(new BookingLogEntry("System", "Database-connected Booking Pipeline ready.", "#64748B"));
        }

        public string GuestId
        {
            get => _guestId;
            set => SetProperty(ref _guestId, value);
        }

        public string RoomId
        {
            get => _roomId;
            set => SetProperty(ref _roomId, value);
        }

        public DateTime CheckIn
        {
            get => _checkIn;
            set => SetProperty(ref _checkIn, value);
        }

        public DateTime CheckOut
        {
            get => _checkOut;
            set => SetProperty(ref _checkOut, value);
        }

        public decimal ExpectedPrice
        {
            get => _expectedPrice;
            set => SetProperty(ref _expectedPrice, value);
        }

        public ObservableCollection<BookingLogEntry> Logs => _logs;

        public ICommand ProcessCommand { get; }
        public ICommand ClearLogsCommand { get; }

        private void ExecuteProcess(object? parameter)
        {
            var request = new BookingProcessRequest(GuestId, RoomId, CheckIn, CheckOut, ExpectedPrice);
            var result = _pipeline.Handle(request);

            string statusText = result.Success ? "SUCCESS" : "FAILED";
            var entry = new BookingLogEntry(
                result.ProcessorName, 
                $"{statusText}: {result.Message}",
                result.StatusColor);

            _logs.Insert(0, entry);
            OnLog?.Invoke($"[Chain] {entry.Author}: {entry.Message}");
        }
    }

    public record BookingLogEntry(string Author, string Message, string Color);
}
