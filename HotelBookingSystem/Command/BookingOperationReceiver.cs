using System.Collections.Generic;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Command
{
    /// <summary>
    /// RECEIVER
    /// Con?ine logica de business reala ?i interac?ioneaza cu repository-urile.
    /// Acesta este cel care "?tie" cum sa execute opera?iunile, separând detaliile 
    /// tehnice de obiectul Comanda care doar îi apeleaza metodele.
    /// </summary>
    public class BookingOperationReceiver
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IBookingService _bookingService;
        private readonly Observer.BookingEventMonitor? _bookingMonitor;

        public BookingOperationReceiver(
            IBookingRepository bookingRepo,
            IRoomRepository roomRepo,
            IBookingService bookingService,
            Observer.BookingEventMonitor? bookingMonitor = null)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _bookingService = bookingService;
            _bookingMonitor = bookingMonitor;
        }

        // -- Booking Operations -----------------------------------------------

        public void SaveBooking(Booking booking)
        {
            _bookingRepo.Save(booking);
            _bookingMonitor?.NotifyBookingCreated(booking);
        }

        public void RemoveBooking(string bookingId)
        {
            // We use a cancelled status as the "removed" state for Undo purposes
            var booking = _bookingRepo.FindById(bookingId);
            if (booking != null)
            {
                booking.Cancel();
                _bookingRepo.Save(booking);
            }
        }

        public void ConfirmBooking(string bookingId)
        {
            _bookingService.ConfirmBooking(bookingId);
            var booking = _bookingRepo.FindById(bookingId);
            if (booking != null)
                _bookingMonitor?.NotifyBookingConfirmed(booking);
        }

        public void RevertBookingToPending(string bookingId)
        {
            // Revert a Confirmed booking back to Pending
            var existing = _bookingRepo.FindById(bookingId);
            if (existing == null) return;

            var pending = new Booking(
                existing.BookingId,
                existing.UserId,
                existing.RoomId,
                existing.CheckInDate,
                existing.CheckOutDate,
                existing.BookingType);

            _bookingRepo.Save(pending);

            // Restore room availability
            var room = _roomRepo.FindById(existing.RoomId);
            if (room != null)
            {
                room.SetAvailability(true);
                _roomRepo.Save(room);
            }
        }

        public void CancelBooking(string bookingId)
        {
            _bookingService.CancelBooking(bookingId);
            var booking = _bookingRepo.FindById(bookingId);
            if (booking != null)
                _bookingMonitor?.NotifyBookingCancelled(booking);
        }

        public void RestoreBookingStatus(string bookingId, BookingStatus previousStatus)
        {
            var existing = _bookingRepo.FindById(bookingId);
            if (existing == null) return;

            var restored = new Booking(
                existing.BookingId,
                existing.UserId,
                existing.RoomId,
                existing.CheckInDate,
                existing.CheckOutDate,
                existing.BookingType);

            if (previousStatus == BookingStatus.Confirmed)
            {
                restored.Confirm();
                var room = _roomRepo.FindById(existing.RoomId);
                if (room != null)
                {
                    room.SetAvailability(false);
                    _roomRepo.Save(room);
                }
            }

            _bookingRepo.Save(restored);

            // Notify observers of the restoration
            if (restored.Status == BookingStatus.Confirmed)
                _bookingMonitor?.NotifyBookingConfirmed(restored);
            else if (restored.Status == BookingStatus.Cancelled)
                _bookingMonitor?.NotifyBookingCancelled(restored);
            else
                _bookingMonitor?.NotifyBookingCreated(restored);
        }

        // -- Room Operations --------------------------------------------------

        public decimal GetRoomBasePrice(string roomId)
        {
            return _roomRepo.FindById(roomId)?.BasePrice ?? 0m;
        }

        public void SetRoomBasePrice(string roomId, decimal newPrice)
        {
            var room = _roomRepo.FindById(roomId);
            if (room == null) return;

            // Create replacement with the new price (immutable BasePrice requires replacement)
            Room updated = room switch
            {
                Models.Suite s => new Models.Suite(
                    s.RoomId, s.RoomNumber, newPrice, s.Capacity,
                    s.HasKitchen, s.HasLivingRoom),

                Models.DeluxeRoom d => new Models.DeluxeRoom(
                    d.RoomId, d.RoomNumber, newPrice, d.Capacity,
                    new List<string>(d.Amenities), d.HasBalcony),

                _ => new Models.StandardRoom(
                    room.RoomId, room.RoomNumber, newPrice, room.Capacity)
            };

            updated.SetAvailability(room.IsAvailable);
            _roomRepo.Save(updated);
        }

        // -- Read-Only Queries ------------------------------------------------

        public Booking? FindBooking(string bookingId) => _bookingRepo.FindById(bookingId);
        public Room? FindRoom(string roomId) => _roomRepo.FindById(roomId);
        public List<Booking> GetAllBookings() => _bookingRepo.GetAllBookings();
        public List<Room> GetAllRooms() => _roomRepo.GetAllRooms();
    }
}

