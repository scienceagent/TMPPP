using System;

namespace HotelBookingSystem.Command
{
     // ══════════════════════════════════════════════════════════════════════════
     // COMMAND INTERFACE
     // Each concrete command encapsulates ONE hotel operation as an object.
     // The Invoker (BookingCommandInvoker) depends only on this abstraction —
     // it never knows whether it is executing a booking creation, confirmation,
     // price adjustment, or a macro composite command.
     //
     // CanUndo lets simple notifications (non-reversible) implement the interface
     // without lying — the Invoker skips Undo for non-undoable commands.
     // ══════════════════════════════════════════════════════════════════════════
     public interface IHotelCommand
     {
          /// <summary>Human-readable label shown in the command history table.</summary>
          string Description { get; }

          /// <summary>Category label for the UI badge (Booking / Room / Payment / Macro).</summary>
          string Category { get; }

          /// <summary>Whether Undo() is supported for this command.</summary>
          bool CanUndo { get; }

          /// <summary>Timestamp set when Execute() is called.</summary>
          DateTime? ExecutedAt { get; }

          /// <summary>
          /// Performs the operation on the Receiver.
          /// Stores all state needed to reverse it before applying changes.
          /// </summary>
          void Execute();

          /// <summary>
          /// Reverses the effect of Execute() exactly.
          /// Only called when CanUndo is true.
          /// </summary>
          void Undo();
     }
}