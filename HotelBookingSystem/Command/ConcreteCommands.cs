using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Command
{
     // ─── Base class — handles timestamp and shared metadata ───────────────────
     public abstract class HotelCommandBase : IHotelCommand
     {
          protected readonly BookingOperationReceiver _receiver;

          protected HotelCommandBase(BookingOperationReceiver receiver)
              => _receiver = receiver;

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

     // ══════════════════════════════════════════════════════════════════════════
     // COMMAND 1 — Create Booking
     // Receiver action : saves the booking to the repository.
     // Undo            : marks the booking as Cancelled + restores room.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class CreateBookingCommand : HotelCommandBase
     {
          private readonly Booking _booking;

          public CreateBookingCommand(BookingOperationReceiver receiver, Booking booking)
              : base(receiver)
          {
               _booking = booking ?? throw new ArgumentNullException(nameof(booking));
          }

          public override string Description =>
              $"Create {_booking.BookingType} booking [{_booking.BookingId[..8]}…]" +
              $" — Room {_receiver.FindRoom(_booking.RoomId)?.RoomNumber ?? "?"}" +
              $" · {(_booking.CheckOutDate - _booking.CheckInDate).Days} nights";

          public override string Category => "Booking";

          protected override void DoExecute()
          {
               _receiver.SaveBooking(_booking);
          }

          public override void Undo()
          {
               // Remove by cancelling — restores room availability via receiver
               _receiver.RemoveBooking(_booking.BookingId);
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // COMMAND 2 — Confirm Booking
     // Receiver action : calls ConfirmBooking on the service (status → Confirmed).
     // Undo            : reverts booking back to Pending + restores room to available.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class ConfirmBookingCommand : HotelCommandBase
     {
          private readonly string _bookingId;
          private BookingStatus _previousStatus;

          public ConfirmBookingCommand(BookingOperationReceiver receiver, string bookingId)
              : base(receiver)
          {
               _bookingId = bookingId;
          }

          public override string Description =>
              $"Confirm booking [{_bookingId[..8]}…]";

          public override string Category => "Booking";

          protected override void DoExecute()
          {
               // Snapshot previous status before mutating
               _previousStatus = _receiver.FindBooking(_bookingId)?.Status
                                 ?? BookingStatus.Pending;
               _receiver.ConfirmBooking(_bookingId);
          }

          public override void Undo()
          {
               _receiver.RevertBookingToPending(_bookingId);
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // COMMAND 3 — Cancel Booking
     // Receiver action : cancels the booking, releases room.
     // Undo            : restores the booking to its previous status.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class CancelBookingCommand : HotelCommandBase
     {
          private readonly string _bookingId;
          private BookingStatus _previousStatus;

          public CancelBookingCommand(BookingOperationReceiver receiver, string bookingId)
              : base(receiver)
          {
               _bookingId = bookingId;
          }

          public override string Description =>
              $"Cancel booking [{_bookingId[..8]}…]";

          public override string Category => "Booking";

          protected override void DoExecute()
          {
               _previousStatus = _receiver.FindBooking(_bookingId)?.Status
                                 ?? BookingStatus.Pending;
               _receiver.CancelBooking(_bookingId);
          }

          public override void Undo()
          {
               _receiver.RestoreBookingStatus(_bookingId, _previousStatus);
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // COMMAND 4 — Adjust Room Base Price
     // Receiver action : replaces the Room object with one with the new price.
     // Undo            : replaces again with the original price (snapshots before).
     //
     // This is a perfect Command use case: the undo is not "subtract the delta"
     // but "restore the exact original value" — like a Memento inside a Command.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class AdjustRoomPriceCommand : HotelCommandBase
     {
          private readonly string _roomId;
          private readonly decimal _newPrice;
          private decimal _oldPrice;      // snapshotted in DoExecute

          public AdjustRoomPriceCommand(BookingOperationReceiver receiver,
                                         string roomId, decimal newPrice)
              : base(receiver)
          {
               _roomId = roomId;
               _newPrice = newPrice;
          }

          private string RoomNumber =>
              _receiver.FindRoom(_roomId)?.RoomNumber ?? _roomId[..8];

          public override string Description =>
              $"Adjust Room {RoomNumber} price → ${_newPrice:F0}/night" +
              (_oldPrice > 0 ? $" (was ${_oldPrice:F0})" : "");

          public override string Category => "Room";

          protected override void DoExecute()
          {
               _oldPrice = _receiver.GetRoomBasePrice(_roomId);   // snapshot
               _receiver.SetRoomBasePrice(_roomId, _newPrice);
          }

          public override void Undo()
          {
               _receiver.SetRoomBasePrice(_roomId, _oldPrice);    // restore exact snapshot
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // COMMAND 5 — Macro: Rebook Guest to Different Room
     // A COMPOSITE command — atomically cancels the original booking and creates
     // a new one for the same guest on a different room.
     //
     // If either step fails the whole operation rolls back — both sub-commands
     // are pushed onto an executed stack and unwound in reverse order.
     //
     // This demonstrates Command used for atomic multi-step transactions:
     // Undo reverts BOTH sub-operations in reverse order.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class RebookGuestCommand : HotelCommandBase
     {
          private readonly string _originalBookingId;
          private readonly Booking _newBooking;
          private readonly Stack<IHotelCommand> _executedSubCommands = new();
          private BookingStatus _originalStatus;

          public RebookGuestCommand(BookingOperationReceiver receiver,
                                     string originalBookingId, Booking newBooking)
              : base(receiver)
          {
               _originalBookingId = originalBookingId;
               _newBooking = newBooking;
          }

          private string OldRoom =>
              _receiver.FindBooking(_originalBookingId) is { } b
                  ? _receiver.FindRoom(b.RoomId)?.RoomNumber ?? "?"
                  : "?";

          private string NewRoom =>
              _receiver.FindRoom(_newBooking.RoomId)?.RoomNumber ?? "?";

          public override string Description =>
              $"Rebook [{_originalBookingId[..8]}…] Room {OldRoom} → Room {NewRoom}";

          public override string Category => "Macro";

          protected override void DoExecute()
          {
               _executedSubCommands.Clear();

               // Step 1: cancel original
               var cancelCmd = new CancelBookingCommand(_receiver, _originalBookingId);
               cancelCmd.Execute();
               _executedSubCommands.Push(cancelCmd);

               // Step 2: create new booking
               var createCmd = new CreateBookingCommand(_receiver, _newBooking);
               createCmd.Execute();
               _executedSubCommands.Push(createCmd);
          }

          public override void Undo()
          {
               // Undo in reverse order (LIFO) — unwind Create, then unwind Cancel
               while (_executedSubCommands.Count > 0)
                    _executedSubCommands.Pop().Undo();
          }
     }
}