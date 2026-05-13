using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBookingSystem.Command
{
    /// <summary>
    /// INVOKER
    /// Gestioneaza execu?ia comenzilor ?i men?ine stivele de Undo ?i Redo.
    /// Este componenta responsabila pentru controlul fluxului de ac?iuni, 
    /// oferind posibilitatea de a anula opera?iuni sau de a rula tranzac?ii complexe.
    /// </summary>
    public class BookingCommandInvoker
    {
        private readonly Stack<IHotelCommand> _undoStack = new();
        private readonly Stack<IHotelCommand> _redoStack = new();
        private readonly List<CommandHistoryEntry> _history = new();

        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;
        public IReadOnlyList<CommandHistoryEntry> History => _history;

        public event Action<string>? OnLog;

        /// <summary>
        /// Executes a single command and adds it to the undo history.
        /// </summary>
        public void Execute(IHotelCommand command)
        {
            try
            {
                command.Execute();
                _undoStack.Push(command);
                _redoStack.Clear(); // New action invalidates redo history

                LogAndAddHistory(command, CommandStatus.Executed, $"EXEC: {command.Description}");
            }
            catch (Exception ex)
            {
                LogAndAddHistory(command, CommandStatus.Failed, $"FAIL: {command.Description} - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Undoes the most recently executed command.
        /// </summary>
        public bool Undo()
        {
            // Skip commands that cannot be undone
            while (_undoStack.Count > 0 && !_undoStack.Peek().CanUndo)
                _undoStack.Pop();

            if (!_undoStack.TryPop(out var command))
            {
                OnLog?.Invoke("[Command] Nothing to undo.");
                return false;
            }

            command.Undo();
            _redoStack.Push(command);

            LogAndAddHistory(command, CommandStatus.Undone, $"UNDO: {command.Description}");
            return true;
        }

        /// <summary>
        /// Redoes the most recently undone command.
        /// </summary>
        public bool Redo()
        {
            if (!_redoStack.TryPop(out var command))
            {
                OnLog?.Invoke("[Command] Nothing to redo.");
                return false;
            }

            command.Execute();
            _undoStack.Push(command);

            LogAndAddHistory(command, CommandStatus.Redone, $"REDO: {command.Description}");
            return true;
        }

        /// <summary>
        /// Reverses all commands currently in the undo history.
        /// </summary>
        public void UndoAll()
        {
            int count = 0;
            while (UndoCount > 0 && Undo()) 
                count++;
            
            OnLog?.Invoke($"[Command] UndoAll: {count} command(s) reversed.");
        }

        /// <summary>
        /// Executes a group of commands as an atomic transaction. 
        /// Rolls back all successfully executed commands if any single one fails.
        /// </summary>
        public bool ExecuteTransaction(IEnumerable<IHotelCommand> commands, string txLabel)
        {
            var executed = new Stack<IHotelCommand>();
            OnLog?.Invoke($"[Command:TRX] -- Begin transaction: {txLabel} --");

            try
            {
                foreach (var cmd in commands)
                {
                    cmd.Execute();
                    executed.Push(cmd);
                    OnLog?.Invoke($"[Command:TRX]   → {cmd.Description}");
                }

                // All succeeded - push to main undo stack
                foreach (var cmd in executed.Reverse())
                {
                    _undoStack.Push(cmd);
                    AddHistoryEntry(cmd, CommandStatus.Executed, $"[TXN: {txLabel}]");
                }
                _redoStack.Clear();

                OnLog?.Invoke($"[Command:TRX] ✓ Transaction committed: {txLabel}");
                return true;
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"[Command:TRX] ✗ Step failed: {ex.Message}");
                RollbackTransaction(executed);
                return false;
            }
        }

        private void RollbackTransaction(Stack<IHotelCommand> executed)
        {
            OnLog?.Invoke($"[Command:TRX] Rolling back {executed.Count} step(s).");

            while (executed.TryPop(out var cmd))
            {
                try
                {
                    cmd.Undo();
                    AddHistoryEntry(cmd, CommandStatus.Undone, "[ROLLBACK]");
                    OnLog?.Invoke($"[Command:TRX]   → Rolled back: {cmd.Description}");
                }
                catch (Exception undoEx)
                {
                    OnLog?.Invoke($"[Command:TRX]   → Rollback failed: {undoEx.Message}");
                }
            }
            OnLog?.Invoke("[Command:TRX] Rollback complete.");
        }

        // -- Helpers -----------------------------------------------------------

        private void LogAndAddHistory(IHotelCommand cmd, CommandStatus status, string logMessage)
        {
            OnLog?.Invoke($"[Command] {logMessage}");
            AddHistoryEntry(cmd, status);
        }

        private void AddHistoryEntry(IHotelCommand cmd, CommandStatus status, string note = "")
        {
            _history.Insert(0, new CommandHistoryEntry(
                Description: cmd.Description,
                Category: cmd.Category,
                Status: status,
                Timestamp: cmd.ExecutedAt ?? DateTime.Now,
                CanUndo: cmd.CanUndo,
                Note: note
            ));

            // Keep history list bounded to prevent memory leaks
            if (_history.Count > 200)
                _history.RemoveAt(_history.Count - 1);
        }
    }

    // -- History entry used for UI binding -------------------------------------

    public enum CommandStatus { Executed, Undone, Redone, Failed }

    public record CommandHistoryEntry(
        string Description,
        string Category,
        CommandStatus Status,
        DateTime Timestamp,
        bool CanUndo,
        string Note = "")
    {
        public string TimestampFmt => Timestamp.ToString("HH:mm:ss");

        public string StatusIcon => Status switch
        {
            CommandStatus.Executed => "✓",
            CommandStatus.Undone => "↶",
            CommandStatus.Redone => "↷",
            CommandStatus.Failed => "✗",
            _ => " "
        };

        public string StatusColor => Status switch
        {
            CommandStatus.Executed => "#15803D",
            CommandStatus.Undone => "#D97706",
            CommandStatus.Redone => "#2563EB",
            CommandStatus.Failed => "#DC2626",
            _ => "#64748B"
        };

        public string CategoryColor => Category switch
        {
            "Booking" => "#2563EB",
            "Room" => "#7C3AED",
            "Payment" => "#15803D",
            "Macro" => "#DC2626",
            _ => "#64748B"
        };

        public string FullDescription => string.IsNullOrEmpty(Note) ? Description : $"{Description}  {Note}";
    }
}

