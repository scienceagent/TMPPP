using System;
using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Command
{
    /// <summary>
    /// CONCRETE COMMANDS
    /// Implementeaza ac?iuni specifice legând un Receiver de un set de parametri.
    /// Fiecare clasa (Create, Confirm, Cancel) ?tie exact ce metoda sa apeleze pe Receiver 
    /// pentru execu?ie ?i cum sa inverseze acea ac?iune în cazul unui Undo.
    /// </summary>
    public abstract class HotelCommandBase : IHotelCommand
    {
        protected readonly BookingOperationReceiver _receiver;

        protected HotelCommandBase(BookingOperationReceiver receiver)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        }

        public abstract string Description { get; }
        public abstract string Category { get; }
        public virtual bool CanUndo => true;
        public DateTime? ExecutedAt { get; private set; }

        public void Execute()
        {
            ExecutedAt = DateTime.Now;
            DoExecute();
        }

        public abstract void Undo();
        
        protected abstract void DoExecute();
    }

    /// <summary>
    /// Command to create and save a new booking.
    /// </summary>
    public class CreateBookingCommand : HotelCommandBase
    {
        private readonly Booking _booking;

        public CreateBookingCommand(BookingOperationReceiver receiver, Booking booking) : base(receiver)
        {
            _booking = booking ?? throw new ArgumentNullException(nameof(booking));
        }

        public override string Description 
        {
            get
            {
                var roomNumber = _receiver.FindRoom(_booking.RoomId)?.RoomNumber ?? "?";
                var nights = (_booking.CheckOutDate - _booking.CheckInDate).Days;
                return $"Create {_booking.BookingType} booking [{_booking.BookingId[..8]}...] - Room {roomNumber} ({nights} nights)";
            }
        }

        public override string Category => "Booking";

        protected override void DoExecute()
        {
            _receiver.SaveBooking(_booking);
        }

        public override void Undo()
        {
            _receiver.RemoveBooking(_booking.BookingId);
        }
    }

    /// <summary>
    /// Command to confirm a pending booking.
    /// </summary>
    public class ConfirmBookingCommand : HotelCommandBase
    {
        private readonly string _bookingId;
        private BookingStatus _previousStatus;

        public ConfirmBookingCommand(BookingOperationReceiver receiver, string bookingId) : base(receiver)
        {
            _bookingId = bookingId;
        }

        public override string Description => $"Confirm booking [{_bookingId[..8]}...]";
        public override string Category => "Booking";

        protected override void DoExecute()
        {
            _previousStatus = _receiver.FindBooking(_bookingId)?.Status ?? BookingStatus.Pending;
            _receiver.ConfirmBooking(_bookingId);
        }

        public override void Undo()
        {
            _receiver.RevertBookingToPending(_bookingId);
        }
    }

    /// <summary>
    /// Command to cancel a booking.
    /// </summary>
    public class CancelBookingCommand : HotelCommandBase
    {
        private readonly string _bookingId;
        private BookingStatus _previousStatus;

        public CancelBookingCommand(BookingOperationReceiver receiver, string bookingId) : base(receiver)
        {
            _bookingId = bookingId;
        }

        public override string Description => $"Cancel booking [{_bookingId[..8]}...]";
        public override string Category => "Booking";

        protected override void DoExecute()
        {
            _previousStatus = _receiver.FindBooking(_bookingId)?.Status ?? BookingStatus.Pending;
            _receiver.CancelBooking(_bookingId);
        }

        public override void Undo()
        {
            _receiver.RestoreBookingStatus(_bookingId, _previousStatus);
        }
    }

    /// <summary>
    /// Command to adjust the base price of a room.
    /// </summary>
    public class AdjustRoomPriceCommand : HotelCommandBase
    {
        private readonly string _roomId;
        private readonly decimal _newPrice;
        private decimal _oldPrice;

        public AdjustRoomPriceCommand(BookingOperationReceiver receiver, string roomId, decimal newPrice) : base(receiver)
        {
            _roomId = roomId;
            _newPrice = newPrice;
        }

        private string RoomNumber => _receiver.FindRoom(_roomId)?.RoomNumber ?? _roomId[..8];

        public override string Description => 
            $"Adjust Room {RoomNumber} price -> ${_newPrice:F0}/night" + 
            (_oldPrice > 0 ? $" (was ${_oldPrice:F0})" : "");

        public override string Category => "Room";

        protected override void DoExecute()
        {
            _oldPrice = _receiver.GetRoomBasePrice(_roomId);
            _receiver.SetRoomBasePrice(_roomId, _newPrice);
        }

        public override void Undo()
        {
            _receiver.SetRoomBasePrice(_roomId, _oldPrice);
        }
    }

    /// <summary>
    /// Macro command that combines cancelling an old booking and creating a new one into a single atomic operation.
    /// </summary>
    public class RebookGuestCommand : HotelCommandBase
    {
        private readonly string _originalBookingId;
        private readonly Booking _newBooking;
        private readonly Stack<IHotelCommand> _executedSubCommands = new();

        public RebookGuestCommand(BookingOperationReceiver receiver, string originalBookingId, Booking newBooking) : base(receiver)
        {
            _originalBookingId = originalBookingId;
            _newBooking = newBooking;
        }

        private string OldRoom
        {
            get
            {
                var booking = _receiver.FindBooking(_originalBookingId);
                return booking != null ? _receiver.FindRoom(booking.RoomId)?.RoomNumber ?? "?" : "?";
            }
        }

        private string NewRoom => _receiver.FindRoom(_newBooking.RoomId)?.RoomNumber ?? "?";

        public override string Description => $"Rebook [{_originalBookingId[..8]}...] Room {OldRoom} -> Room {NewRoom}";
        public override string Category => "Macro";

        protected override void DoExecute()
        {
            _executedSubCommands.Clear();

            // Step 1: Cancel the original booking
            var cancelCmd = new CancelBookingCommand(_receiver, _originalBookingId);
            cancelCmd.Execute();
            _executedSubCommands.Push(cancelCmd);

            // Step 2: Create the new booking
            var createCmd = new CreateBookingCommand(_receiver, _newBooking);
            createCmd.Execute();
            _executedSubCommands.Push(createCmd);
        }

        public override void Undo()
        {
            // Undo in reverse order (LIFO) - unwind Create, then unwind Cancel
            while (_executedSubCommands.TryPop(out var command))
            {
                command.Undo();
            }
        }
    }
}

