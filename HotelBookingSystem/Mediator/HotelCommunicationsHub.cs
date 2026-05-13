using System;
using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Mediator
{
    public class HotelCommunicationsHub : IHotelMediator
    {
        private ReceptionComponent _reception;
        private HousekeepingComponent _housekeeping;
        private AccountingComponent _accounting;

        public event Action<string, string>? OnBroadcast;

        public void RegisterComponents(ReceptionComponent r, HousekeepingComponent h, AccountingComponent a)
        {
            _reception = r;
            _housekeeping = h;
            _accounting = a;
        }

        public void Notify(object sender, string @event, object? data = null)
        {
            switch (@event)
            {
                case "GuestCheckedOut":
                    if (data is Booking booking)
                    {
                        Broadcast("Reception", $"Guest {booking.UserId} checked out. Notifying Housekeeping and Accounting.");
                        _housekeeping.CleanRoom(booking.RoomId);
                        _accounting.ProcessFinalBill(booking);
                    }
                    break;

                case "RoomCleaned":
                    if (data is Room room)
                    {
                        Broadcast("Housekeeping", $"Room {room.RoomNumber} is now CLEAN and ready.");
                        _reception.AssignRoom(room.RoomId);
                    }
                    break;

                case "PaymentFinalized":
                    Broadcast("Accounting", $"Payment for booking {data} has been confirmed. Record closed.");
                    break;
            }
        }

        private void Broadcast(string sender, string message)
        {
            OnBroadcast?.Invoke(sender, message);
        }
    }
}
