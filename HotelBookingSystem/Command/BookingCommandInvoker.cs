using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBookingSystem.Command
{
     // ══════════════════════════════════════════════════════════════════════════
     // INVOKER
     // BookingCommandInvoker is the single entry point for executing hotel
     // operations. It knows nothing about what the commands do — it only
     // calls Execute() and Undo() through the IHotelCommand interface.
     //
     // Responsibilities:
     //   • Execute commands and push them to the undo history
     //   • Undo the most recent command (pop from undo stack, push to redo stack)
     //   • Redo the most recently undone command
     //   • Execute atomic transactions — full rollback on any step failure
     //   • Maintain a complete flat history for the audit UI
     //
     // The Invoker is decoupled from both the commands and the receivers.
     // Adding a new command type requires ZERO changes here.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class BookingCommandInvoker
     {
          private readonly Stack<IHotelCommand> _undoStack = new();
          private readonly Stack<IHotelCommand> _redoStack = new();
          private readonly List<CommandHistoryEntry> _history = new();

          public int UndoCount => _undoStack.Count;
          public int RedoCount => _redoStack.Count;
          public IReadOnlyList<CommandHistoryEntry> History => _history;

          public event Action<string>? OnLog;

          // ── Execute a single command ──────────────────────────────────────────
          public void Execute(IHotelCommand command)
          {
               try
               {
                    command.Execute();
                    _undoStack.Push(command);
                    _redoStack.Clear();   // new action invalidates redo history

                    AddHistory(command, CommandStatus.Executed);
                    OnLog?.Invoke($"[Command] ▶ EXEC: {command.Description}");
               }
               catch (Exception ex)
               {
                    AddHistory(command, CommandStatus.Failed, ex.Message);
                    OnLog?.Invoke($"[Command] ✗ FAIL: {command.Description} — {ex.Message}");
                    throw;
               }
          }

          // ── Undo the most recent undoable command ─────────────────────────────
          public bool Undo()
          {
               while (_undoStack.Count > 0 && !_undoStack.Peek().CanUndo)
                    _undoStack.Pop();   // skip non-undoable entries

               if (_undoStack.Count == 0)
               {
                    OnLog?.Invoke("[Command] Nothing to undo.");
                    return false;
               }

               var command = _undoStack.Pop();
               command.Undo();
               _redoStack.Push(command);

               AddHistory(command, CommandStatus.Undone);
               OnLog?.Invoke($"[Command] ↩ UNDO: {command.Description}");
               return true;
          }

          // ── Redo the most recently undone command ─────────────────────────────
          public bool Redo()
          {
               if (_redoStack.Count == 0)
               {
                    OnLog?.Invoke("[Command] Nothing to redo.");
                    return false;
               }

               var command = _redoStack.Pop();
               command.Execute();
               _undoStack.Push(command);

               AddHistory(command, CommandStatus.Redone);
               OnLog?.Invoke($"[Command] ↪ REDO: {command.Description}");
               return true;
          }

          // ── Atomic transaction — full rollback on any failure ─────────────────
          public bool ExecuteTransaction(IEnumerable<IHotelCommand> commands, string txLabel)
          {
               var executed = new Stack<IHotelCommand>();
               OnLog?.Invoke($"[Command:TRX] ── Begin transaction: {txLabel} ──");

               try
               {
                    foreach (var cmd in commands)
                    {
                         cmd.Execute();
                         executed.Push(cmd);
                         OnLog?.Invoke($"[Command:TRX]   ▶ {cmd.Description}");
                    }

                    // All succeeded — push all to undo history as a group
                    foreach (var cmd in executed.Reverse())
                    {
                         _undoStack.Push(cmd);
                         AddHistory(cmd, CommandStatus.Executed, $"[TXN: {txLabel}]");
                    }
                    _redoStack.Clear();

                    OnLog?.Invoke($"[Command:TRX] ✓ Transaction committed: {txLabel}");
                    return true;
               }
               catch (Exception ex)
               {
                    OnLog?.Invoke($"[Command:TRX] ✗ Step failed: {ex.Message}");
                    OnLog?.Invoke($"[Command:TRX] Rolling back {executed.Count} step(s)…");

                    while (executed.Count > 0)
                    {
                         var cmd = executed.Pop();
                         try
                         {
                              cmd.Undo();
                              AddHistory(cmd, CommandStatus.Undone, "[ROLLBACK]");
                              OnLog?.Invoke($"[Command:TRX]   ↩ Rolled back: {cmd.Description}");
                         }
                         catch (Exception undoEx)
                         {
                              OnLog?.Invoke($"[Command:TRX]   ⚠ Rollback failed: {undoEx.Message}");
                         }
                    }

                    OnLog?.Invoke($"[Command:TRX] Rollback complete.");
                    return false;
               }
          }

          // ── Undo everything in history ────────────────────────────────────────
          public void UndoAll()
          {
               int count = 0;
               while (_undoStack.Count > 0)
               {
                    if (Undo()) count++;
                    else break;
               }
               OnLog?.Invoke($"[Command] UndoAll: {count} command(s) reversed.");
          }

          // ── Helpers ───────────────────────────────────────────────────────────
          private void AddHistory(IHotelCommand cmd, CommandStatus status, string note = "")
          {
               _history.Insert(0, new CommandHistoryEntry(   // newest first
                   Description: cmd.Description,
                   Category: cmd.Category,
                   Status: status,
                   Timestamp: cmd.ExecutedAt ?? DateTime.Now,
                   CanUndo: cmd.CanUndo,
                   Note: note
               ));

               // Keep history bounded
               if (_history.Count > 200)
                    _history.RemoveAt(_history.Count - 1);
          }
     }

     // ── History entry (shown in the UI table) ──────────────────────────────────
     public enum CommandStatus { Executed, Undone, Redone, Failed }

     public sealed record CommandHistoryEntry(
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
               CommandStatus.Executed => "▶",
               CommandStatus.Undone => "↩",
               CommandStatus.Redone => "↪",
               CommandStatus.Failed => "✗",
               _ => "•"
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

          public string FullDescription =>
              string.IsNullOrEmpty(Note) ? Description : $"{Description}  {Note}";
     }
}