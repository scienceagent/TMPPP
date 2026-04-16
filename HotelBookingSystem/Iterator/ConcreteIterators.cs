using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Iterator
{
     // ─── Base class — position tracking + defensive bounds ───────────────────
     public abstract class BookingIteratorBase : IBookingIterator
     {
          protected IReadOnlyList<Booking> _items;
          protected int _position = 0;

          protected BookingIteratorBase(IReadOnlyList<Booking> items)
              => _items = items;

          public int CurrentIndex => _position;
          public int TotalCount => _items.Count;

          public bool HasNext() => _position < _items.Count;
          public Booking Next()
          {
               if (!HasNext())
                    throw new InvalidOperationException(
                        $"[{IteratorName}] No more elements — call Reset() to restart.");
               return _items[_position++];
          }

          public Booking? Peek() => HasNext() ? _items[_position] : null;
          public void Reset() => _position = 0;

          public abstract string IteratorName { get; }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // ITERATOR 1 — Sequential (insertion order)
     // Simplest traversal — bookings exactly as they were created.
     // Used by: audit log, creation-order reports.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class SequentialBookingIterator : BookingIteratorBase
     {
          public SequentialBookingIterator(IReadOnlyList<Booking> items)
              : base(items) { }

          public override string IteratorName => "Sequential (creation order)";
     }

     // ══════════════════════════════════════════════════════════════════════════
     // ITERATOR 2 — Chronological (sorted by check-in date)
     // Builds a new sorted view at construction time — the original list is unchanged.
     // Used by: occupancy timeline, availability planning.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class ChronologicalBookingIterator : BookingIteratorBase
     {
          public ChronologicalBookingIterator(IEnumerable<Booking> source)
              : base(source.OrderBy(b => b.CheckInDate).ToList()) { }

          public override string IteratorName => "Chronological (check-in date ASC)";
     }

     // ══════════════════════════════════════════════════════════════════════════
     // ITERATOR 3 — Status Filter
     // Yields only bookings whose Status matches the requested value.
     // HasNext() advances past non-matching elements before answering.
     // Used by: Confirmed-only revenue reports, Pending review lists.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class StatusFilterIterator : IBookingIterator
     {
          private readonly IReadOnlyList<Booking> _source;
          private readonly BookingStatus _status;
          private int _position = 0;
          private List<Booking>? _filtered;

          public StatusFilterIterator(IReadOnlyList<Booking> source, BookingStatus status)
          {
               _source = source;
               _status = status;
               BuildFiltered();
          }

          private void BuildFiltered()
              => _filtered = _source.Where(b => b.Status == _status).ToList();

          public string IteratorName => $"Status Filter ({_status})";
          public int CurrentIndex => _position;
          public int TotalCount => _filtered!.Count;

          public bool HasNext() => _position < _filtered!.Count;
          public Booking Next()
          {
               if (!HasNext()) throw new InvalidOperationException("No more elements.");
               return _filtered![_position++];
          }
          public Booking? Peek() => HasNext() ? _filtered![_position] : null;
          public void Reset() => _position = 0;
     }

     // ══════════════════════════════════════════════════════════════════════════
     // ITERATOR 4 — Date Range Filter
     // Yields bookings whose CheckInDate falls within [from, to] inclusive.
     // Sorted by check-in date for readability.
     // Used by: weekly/monthly occupancy reports, seasonal analysis.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class DateRangeIterator : BookingIteratorBase
     {
          private readonly DateTime _from;
          private readonly DateTime _to;

          public DateRangeIterator(IEnumerable<Booking> source, DateTime from, DateTime to)
              : base(source
                     .Where(b => b.CheckInDate.Date >= from.Date && b.CheckInDate.Date <= to.Date)
                     .OrderBy(b => b.CheckInDate)
                     .ToList())
          {
               _from = from;
               _to = to;
          }

          public override string IteratorName =>
              $"Date Range ({_from:dd MMM} – {_to:dd MMM yyyy})";
     }

     // ══════════════════════════════════════════════════════════════════════════
     // ITERATOR 5 — Recent N Bookings (lazy stop)
     // Returns only the last N bookings (most recently created first).
     // This is the lazy variant — it stops after N elements without materialising
     // the full collection. Demonstrates iterator as a lazy pipeline.
     // Used by: dashboard "recent activity", quick overview widgets.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class RecentBookingsIterator : IBookingIterator
     {
          private readonly IReadOnlyList<Booking> _recent;    // last N, newest first
          private int _position = 0;
          private readonly int _count;

          public RecentBookingsIterator(IReadOnlyList<Booking> source, int count)
          {
               _count = Math.Max(1, count);
               _recent = source
                         .AsEnumerable()
                         .Reverse()                // newest first
                         .Take(_count)
                         .ToList();
          }

          public string IteratorName => $"Recent ({_count} bookings, newest first)";
          public int CurrentIndex => _position;
          public int TotalCount => _recent.Count;

          public bool HasNext() => _position < _recent.Count;
          public Booking Next()
          {
               if (!HasNext()) throw new InvalidOperationException("No more elements.");
               return _recent[_position++];
          }
          public Booking? Peek() => HasNext() ? _recent[_position] : null;
          public void Reset() => _position = 0;
     }

     // ══════════════════════════════════════════════════════════════════════════
     // ITERATOR 6 — Booking Type Filter
     // Yields only bookings of a specific type (Standard / Premium / VIP).
     // Used by: type-specific revenue reports, VIP guest lists.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class TypeFilterIterator : BookingIteratorBase
     {
          private readonly string _bookingType;

          public TypeFilterIterator(IEnumerable<Booking> source, string bookingType)
              : base(source
                     .Where(b => string.Equals(b.BookingType, bookingType,
                                                StringComparison.OrdinalIgnoreCase))
                     .OrderBy(b => b.CheckInDate)
                     .ToList())
          {
               _bookingType = bookingType;
          }

          public override string IteratorName => $"Type Filter ({_bookingType})";
     }
}