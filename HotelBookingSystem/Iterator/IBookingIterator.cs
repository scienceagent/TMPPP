using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Iterator
{
     // ══════════════════════════════════════════════════════════════════════════
     // ITERATOR INTERFACE
     // Defines the traversal contract that every concrete iterator must satisfy.
     // The client (BookingReportEngine) depends ONLY on this abstraction —
     // it never knows whether it is iterating a list, a sorted view, a date-
     // filtered slice, or any other traversal strategy.
     //
     // Design choices:
     //   • HasNext() + Next() is the classic GoF form — explicit and clear in reports
     //   • Reset() allows reuse of the same iterator without creating a new one
     //   • CurrentIndex / TotalCount let the report engine show progress
     //   • Peek() lets the engine look ahead without consuming the element
     // ══════════════════════════════════════════════════════════════════════════
     public interface IBookingIterator
     {
          /// <summary>Returns true if there is at least one more element.</summary>
          bool HasNext();

          /// <summary>Returns the next booking and advances the internal cursor.</summary>
          Booking Next();

          /// <summary>Resets the cursor to the beginning — allows re-traversal.</summary>
          void Reset();

          /// <summary>Returns the next booking WITHOUT advancing the cursor.</summary>
          Booking? Peek();

          /// <summary>Zero-based index of the element that will be returned by the next Next() call.</summary>
          int CurrentIndex { get; }

          /// <summary>Total number of elements this iterator will yield (may be evaluated lazily).</summary>
          int TotalCount { get; }

          /// <summary>Human-readable description of this traversal strategy.</summary>
          string IteratorName { get; }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // AGGREGATE INTERFACE
     // The collection declares factory methods that produce iterators.
     // The client asks the collection for a specific traversal — but the
     // collection keeps its internal structure (List<Booking>, sort order,
     // index) completely hidden.
     // ══════════════════════════════════════════════════════════════════════════
     public interface IBookingCollection
     {
          // ── Standard traversals ───────────────────────────────────────────────

          /// <summary>All bookings in insertion (creation) order.</summary>
          IBookingIterator CreateSequentialIterator();

          /// <summary>All bookings sorted by check-in date ascending.</summary>
          IBookingIterator CreateChronologicalIterator();

          /// <summary>Only bookings with the specified status.</summary>
          IBookingIterator CreateStatusFilterIterator(BookingStatus status);

          /// <summary>
          /// Bookings whose check-in falls within [from, to] (inclusive).
          /// </summary>
          IBookingIterator CreateDateRangeIterator(
              System.DateTime from, System.DateTime to);

          /// <summary>
          /// Returns the most recently created N bookings (lazy stop after N).
          /// </summary>
          IBookingIterator CreateRecentIterator(int count);

          /// <summary>
          /// Bookings of a specific type (Standard / Premium / VIP).
          /// </summary>
          IBookingIterator CreateTypeFilterIterator(string bookingType);

          // ── C# idiomatic — implements IEnumerable for native foreach ──────────
          System.Collections.Generic.IEnumerable<Booking> AsEnumerable();
     }
}