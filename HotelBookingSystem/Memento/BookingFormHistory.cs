using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBookingSystem.Memento
{
     // ══════════════════════════════════════════════════════════════════════════
     // CARETAKER — BookingFormHistory
     //
     // Manages the stack of BookingFormSnapshot objects.
     // It treats every snapshot as an OPAQUE token — it stores and returns
     // them but never reads or modifies their internal form-field state.
     // The Caretaker only reads the Memento's metadata (Label, SavedAt, Summary)
     // for display purposes; all field restoration goes through Originator.Restore().
     //
     // Features beyond the textbook example:
     //   • Separate Undo and Redo stacks (standard)
     //   • Named checkpoints — staff can give a snapshot a meaningful name
     //     and jump directly to any checkpoint in the timeline
     //   • Max 50 snapshots to prevent unbounded memory growth
     //   • AutoSave: saves a checkpoint before every Undo/Redo so the current
     //     state is never permanently lost
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class BookingFormHistory
     {
          private const int MaxSnapshots = 50;

          private readonly Stack<BookingFormSnapshot> _undoStack = new();
          private readonly Stack<BookingFormSnapshot> _redoStack = new();

          // Named checkpoints — stored separately, always preserved
          private readonly List<NamedCheckpoint> _namedCheckpoints = new();

          public int UndoCount => _undoStack.Count;
          public int RedoCount => _redoStack.Count;
          public int NamedCount => _namedCheckpoints.Count;

          public IReadOnlyList<BookingFormSnapshot> UndoHistory =>
              _undoStack.ToList().AsReadOnly();

          public IReadOnlyList<NamedCheckpoint> NamedCheckpoints =>
              _namedCheckpoints.AsReadOnly();

          public event Action<string>? OnLog;

          // ── SAVE — push current state onto the undo stack ─────────────────────
          // Call this whenever the user changes a field and you want a restore point.
          public void SaveState(BookingFormOriginator originator, string? label = null)
          {
               var snapshot = originator.Save(label);
               _undoStack.Push(snapshot);

               // Trim if we exceed the limit (oldest = bottom of stack → rebuild)
               if (_undoStack.Count > MaxSnapshots)
                    TrimToMax();

               // Any new state invalidates the redo path
               _redoStack.Clear();

               OnLog?.Invoke($"[Memento] Checkpoint saved: {snapshot.Label}");
          }

          // ── UNDO — restore the previous snapshot ─────────────────────────────
          public bool Undo(BookingFormOriginator originator)
          {
               if (_undoStack.Count == 0)
               {
                    OnLog?.Invoke("[Memento] Nothing to undo.");
                    return false;
               }

               // Before restoring: save the CURRENT state onto the redo stack
               // so we can get back to "now" with Redo()
               var currentSnapshot = originator.Save("Before Undo");
               _redoStack.Push(currentSnapshot);

               var target = _undoStack.Pop();
               originator.Restore(target);

               OnLog?.Invoke($"[Memento] Undo → restored to: {target.Label} [{target.TimestampFmt}]");
               return true;
          }

          // ── REDO — restore the next snapshot (undo the undo) ─────────────────
          public bool Redo(BookingFormOriginator originator)
          {
               if (_redoStack.Count == 0)
               {
                    OnLog?.Invoke("[Memento] Nothing to redo.");
                    return false;
               }

               // Save current state to undo stack before going forward
               var current = originator.Save("Before Redo");
               _undoStack.Push(current);

               var target = _redoStack.Pop();
               originator.Restore(target);

               OnLog?.Invoke($"[Memento] Redo → restored to: {target.Label} [{target.TimestampFmt}]");
               return true;
          }

          // ── NAMED CHECKPOINTS ─────────────────────────────────────────────────

          /// <summary>
          /// Creates a named checkpoint — staff can label key moments like
          /// "Guest details complete" or "Dates agreed with guest".
          /// Named checkpoints are preserved even when the undo stack overflows.
          /// </summary>
          public void SaveNamedCheckpoint(BookingFormOriginator originator, string name)
          {
               if (string.IsNullOrWhiteSpace(name)) name = $"Checkpoint {_namedCheckpoints.Count + 1}";

               var snapshot = originator.Save(name);
               _namedCheckpoints.Insert(0, new NamedCheckpoint(name, snapshot)); // newest first

               // Also push to undo stack so it participates in normal Undo flow
               _undoStack.Push(snapshot);
               _redoStack.Clear();

               OnLog?.Invoke($"[Memento] Named checkpoint saved: '{name}'");
          }

          /// <summary>
          /// Jump directly to any named checkpoint — bypasses the sequential
          /// undo stack and restores the exact named state.
          /// </summary>
          public bool JumpToCheckpoint(BookingFormOriginator originator, string checkpointName)
          {
               var cp = _namedCheckpoints.FirstOrDefault(c => c.Name == checkpointName);
               if (cp == null)
               {
                    OnLog?.Invoke($"[Memento] Checkpoint '{checkpointName}' not found.");
                    return false;
               }

               // Save current state to undo so the jump itself is undoable
               _undoStack.Push(originator.Save($"Before jump to '{checkpointName}'"));
               _redoStack.Clear();

               originator.Restore(cp.Snapshot);
               OnLog?.Invoke($"[Memento] Jumped to checkpoint: '{checkpointName}' [{cp.Snapshot.TimestampFmt}]");
               return true;
          }

          // ── CLEAR ─────────────────────────────────────────────────────────────
          public void ClearAll()
          {
               _undoStack.Clear();
               _redoStack.Clear();
               _namedCheckpoints.Clear();
               OnLog?.Invoke("[Memento] History cleared.");
          }

          // ── Trim undo stack to MaxSnapshots (keep newest) ─────────────────────
          private void TrimToMax()
          {
               var items = _undoStack.ToArray();   // ToArray = newest first
               _undoStack.Clear();
               foreach (var item in items.Take(MaxSnapshots))
                    _undoStack.Push(item);
          }
     }

     // ── Named checkpoint entry ─────────────────────────────────────────────────
     public sealed class NamedCheckpoint
     {
          public string Name { get; }
          public BookingFormSnapshot Snapshot { get; }
          public string TimeFmt => Snapshot.TimestampFmt;
          public string AgeFmt => Snapshot.AgeLabel;
          public string Summary => Snapshot.Summary;

          public NamedCheckpoint(string name, BookingFormSnapshot snapshot)
          {
               Name = name;
               Snapshot = snapshot;
          }
     }
}