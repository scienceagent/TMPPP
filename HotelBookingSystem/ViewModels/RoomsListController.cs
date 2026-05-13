using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models.User;
using HotelBookingSystem.Models;
using HotelBookingSystem.Commands;

namespace HotelBookingSystem.ViewModels
{
    public class RoomsListController : BaseViewModel
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;

        public ObservableCollection<RoomItemViewModel> AvailableRooms { get; } = new();
        public ObservableCollection<RoomItemViewModel> AssignedRooms { get; } = new();

        public ICommand RefreshCommand { get; }

        public RoomsListController(IRoomRepository roomRepository, IBookingRepository bookingRepository, IUserRepository userRepository)
        {
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;

            RefreshCommand = new RelayCommand(_ => Refresh());
            Refresh();
        }

        public void Refresh()
        {
            AvailableRooms.Clear();
            AssignedRooms.Clear();

            var allRooms = _roomRepository.GetAllRooms();
            var allBookings = _bookingRepository.GetAllBookings();

            foreach (var room in allRooms)
            {
                // Check if room is assigned to a guest (active booking)
                var activeBooking = allBookings.FirstOrDefault(b => b.RoomId == room.RoomId && 
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn || b.Status == BookingStatus.Pending));

                if (activeBooking != null)
                {
                    var guest = _userRepository.FindById(activeBooking.UserId);
                    AssignedRooms.Add(new RoomItemViewModel(room, guest, activeBooking));
                }
                else
                {
                    AvailableRooms.Add(new RoomItemViewModel(room, null, null));
                }
            }
            
            OnPropertyChanged(nameof(AvailableRoomsCount));
            OnPropertyChanged(nameof(AssignedRoomsCount));
        }

        public int AvailableRoomsCount => AvailableRooms.Count;
        public int AssignedRoomsCount => AssignedRooms.Count;
    }

    public class RoomItemViewModel : BaseViewModel
    {
        public Room Room { get; }
        public Guest? Guest { get; }
        public Booking? Booking { get; }

        public string RoomNumber => Room.RoomNumber;
        public string RoomType => Room.GetType().Name.Replace("Room", "");
        public decimal Price => Room.BasePrice;
        public string ImagePath => Room.ImagePath;
        public string Status => Booking?.Status.ToString() ?? "Available";
        public string GuestName => Guest?.Name ?? "N/A";
        public string Duration => Booking != null ? $"{Booking.CheckInDate:dd MMM} - {Booking.CheckOutDate:dd MMM}" : "N/A";

        public RoomItemViewModel(Room room, User? guest, Booking? booking)
        {
            Room = room;
            Guest = guest as Guest;
            Booking = booking;
        }
    }
}
