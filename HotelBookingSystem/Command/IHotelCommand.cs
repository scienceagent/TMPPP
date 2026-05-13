using System;

namespace HotelBookingSystem.Command
{
    /// <summary>
    /// COMMAND INTERFACE
    /// Define?te opera?iile de baza (Execute ?i Undo) pe care orice ac?iune de hotel 
    /// trebuie sa le implementeze. Aceasta abstrac?ie permite Invoker-ului sa gestioneze 
    /// orice comanda (Booking, Room, Payment) într-un mod uniform pentru istoricul Undo/Redo.
    /// </summary>
    public interface IHotelCommand
    {
        /// <summary>Short description of the command for logs and UI.</summary>
        string Description { get; }

        /// <summary>Category of the command (e.g., Booking, Room).</summary>
        string Category { get; }

        /// <summary>Indicates whether the action can be reversed (Undo).</summary>
        bool CanUndo { get; }

        /// <summary>Timestamp of when the command was executed.</summary>
        DateTime? ExecutedAt { get; }

        /// <summary>
        /// Executes the action of the command.
        /// </summary>
        void Execute();

        /// <summary>
        /// Reverses the effects of the Execute method.
        /// </summary>
        void Undo();
    }
}

