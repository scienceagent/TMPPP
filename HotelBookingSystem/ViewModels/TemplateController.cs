using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelBookingSystem.TemplateMethod;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using System.Linq;

namespace HotelBookingSystem.ViewModels
{
    public class TemplateController : BaseViewModel
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IRoomRepository _roomRepo;
        
        private Booking? _selectedBooking;
        private string _generatedInvoice = "";
        private string _selectedFormat = "Email";

        public TemplateController(IBookingRepository bookingRepo, IRoomRepository roomRepo)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;

            GenerateCommand = new RelayCommand(_ => ExecuteGeneration());
            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        public ObservableCollection<Booking> AllBookings { get; } = new ObservableCollection<Booking>();
        
        public Booking? SelectedBooking
        {
            get => _selectedBooking;
            set => SetProperty(ref _selectedBooking, value);
        }

        public ObservableCollection<string> Formats { get; } = new ObservableCollection<string> { "Email", "PDF" };
        
        public string SelectedFormat
        {
            get => _selectedFormat;
            set => SetProperty(ref _selectedFormat, value);
        }

        public string GeneratedInvoice
        {
            get => _generatedInvoice;
            set => SetProperty(ref _generatedInvoice, value);
        }

        public ICommand GenerateCommand { get; }
        public ICommand RefreshCommand { get; }

        private void ExecuteGeneration()
        {
            if (SelectedBooking == null)
            {
                GeneratedInvoice = "⚠ No booking selected. Create a booking first via New Booking, then click Refresh.";
                return;
            }

            var room = _roomRepo.FindById(SelectedBooking.RoomId);
            if (room == null)
            {
                GeneratedInvoice = "⚠ Room data not found for the selected booking.";
                return;
            }

            InvoiceGenerator generator = SelectedFormat switch
            {
                "PDF" => new PdfInvoiceGenerator(),
                _ => new EmailInvoiceGenerator()
            };

            GeneratedInvoice = generator.GenerateInvoice(SelectedBooking, room);
        }

        private void LoadData()
        {
            var bookings = _bookingRepo.GetAllBookings();
            AllBookings.Clear();
            foreach (var b in bookings) AllBookings.Add(b);
            
            if (AllBookings.Any()) SelectedBooking = AllBookings.First();
        }
    }
}
