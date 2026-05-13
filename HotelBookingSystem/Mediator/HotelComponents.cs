using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Mediator
{
    // ─── RECEPTION COMPONENT ─────────────────────────────────────────────
    public class ReceptionComponent : HotelComponent
    {
        private readonly IBookingRepository _bookingRepo;

        public ReceptionComponent(IHotelMediator mediator, IBookingRepository bookingRepo) : base(mediator)
        {
            _bookingRepo = bookingRepo;
        }

        public void CheckOut(string bookingId)
        {
            var booking = _bookingRepo.FindById(bookingId);
            if (booking != null)
            {
                Console.WriteLine($"[Reception] Processing check-out for {bookingId}");
                _mediator.Notify(this, "GuestCheckedOut", booking);
            }
        }

        public void AssignRoom(string roomId)
        {
            Console.WriteLine($"[Reception] Room {roomId} assigned to a new guest.");
        }
    }

    // ─── HOUSEKEEPING COMPONENT ──────────────────────────────────────────
    public class HousekeepingComponent : HotelComponent
    {
        private readonly IRoomRepository _roomRepo;

        public HousekeepingComponent(IHotelMediator mediator, IRoomRepository roomRepo) : base(mediator)
        {
            _roomRepo = roomRepo;
        }

        public void CleanRoom(string roomId)
        {
            var room = _roomRepo.FindById(roomId);
            if (room != null)
            {
                Console.WriteLine($"[Housekeeping] Cleaning room {room.RoomNumber}...");
                // Simulate cleaning time
                _mediator.Notify(this, "RoomCleaned", room);
            }
        }
    }

    // ─── ACCOUNTING COMPONENT ────────────────────────────────────────────
    public class AccountingComponent : HotelComponent
    {
        public AccountingComponent(IHotelMediator mediator) : base(mediator) { }

        public void ProcessFinalBill(Booking booking)
        {
            Console.WriteLine($"[Accounting] Generating final bill for {booking.BookingId}. Total processed.");
            _mediator.Notify(this, "PaymentFinalized", booking.BookingId);
        }
    }
}
