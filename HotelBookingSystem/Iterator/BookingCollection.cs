using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Iterator
{
     // ══════════════════════════════════════════════════════════════════════════
     // CONCRETE AGGREGATE — BookingCollection
     // Wraps IBookingRepository and exposes factory methods that produce
     // typed iterators. The caller never sees a List<Booking>, IQueryable, or
     // any other internal structure — only the IBookingIterator abstraction.
     //
     // Also implements IEnumerable<Booking> so native C# foreach works:
     //   foreach (var booking in collection) { ... }
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class BookingCollection : IBookingCollection, IEnumerable<Booking>
     {
          private readonly IBookingRepository _repository;

          public BookingCollection(IBookingRepository repository)
              => _repository = repository;

          // ── Snapshot helper — always get a fresh list from the repo ───────────
          private IReadOnlyList<Booking> Snapshot()
              => _repository.GetAllBookings().AsReadOnly();

          // ── Aggregate interface — iterator factory methods ────────────────────

          public IBookingIterator CreateSequentialIterator()
              => new SequentialBookingIterator(Snapshot());

          public IBookingIterator CreateChronologicalIterator()
              => new ChronologicalBookingIterator(Snapshot());

          public IBookingIterator CreateStatusFilterIterator(BookingStatus status)
              => new StatusFilterIterator(Snapshot(), status);

          public IBookingIterator CreateDateRangeIterator(DateTime from, DateTime to)
              => new DateRangeIterator(Snapshot(), from, to);

          public IBookingIterator CreateRecentIterator(int count)
              => new RecentBookingsIterator(Snapshot(), count);

          public IBookingIterator CreateTypeFilterIterator(string bookingType)
              => new TypeFilterIterator(Snapshot(), bookingType);

          // ── C# IEnumerable — enables: foreach (var b in collection) ──────────
          public IEnumerable<Booking> AsEnumerable()
              => _repository.GetAllBookings();

          public IEnumerator<Booking> GetEnumerator()
              => _repository.GetAllBookings().GetEnumerator();

          System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
              => GetEnumerator();

          // ── Convenience properties ─────────────────────────────────────────────
          public int TotalCount => _repository.GetAllBookings().Count;
     }

     // ══════════════════════════════════════════════════════════════════════════
     // CLIENT — BookingReportEngine
     // Uses iterators exclusively — never accesses List<Booking> directly,
     // never calls LINQ on a raw list, never knows the repository's internals.
     // All traversal is done through IBookingIterator.HasNext() / Next().
     //
     // This is the key separation: the REPORT ENGINE knows how to format a
     // report; the ITERATORS know how to traverse the collection. Neither
     // knows about the other's implementation.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class BookingReportEngine
     {
          private static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");
          private static string Usd(decimal v) => v.ToString("C", En);

          // ── REPORT 1: Full Summary ────────────────────────────────────────────
          // Uses: SequentialBookingIterator
          // Traverses all bookings in creation order — counts by status, sums revenue.
          public string GenerateSummaryReport(IBookingCollection collection,
                                               IUserRepository userRepo,
                                               IRoomRepository roomRepo)
          {
               var iter = collection.CreateSequentialIterator();
               var sb = new StringBuilder();

               sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
               sb.AppendLine("║           GRAND HORIZON HOTEL — BOOKING SUMMARY          ║");
               sb.AppendLine("╚══════════════════════════════════════════════════════════╝");
               sb.AppendLine($"  Generated : {DateTime.Now:dd MMM yyyy  HH:mm:ss}");
               sb.AppendLine($"  Iterator  : {iter.IteratorName}");
               sb.AppendLine($"  Total     : {iter.TotalCount} booking(s)");
               sb.AppendLine(new string('─', 64));

               int pending = 0, confirmed = 0, cancelled = 0;
               decimal revenue = 0m;
               int processed = 0;

               while (iter.HasNext())        // ← iterator drives the loop
               {
                    var b = iter.Next();
                    var room = roomRepo.FindById(b.RoomId);
                    var user = userRepo.FindById(b.UserId);
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;
                    decimal value = (room?.BasePrice ?? 0m) * nights;

                    sb.AppendLine(
                        $"  [{++processed,2}] {b.BookingId[..8]}… · " +
                        $"{b.BookingType,-9} · {b.Status,-10} · " +
                        $"{b.CheckInDate:dd MMM} → {b.CheckOutDate:dd MMM} " +
                        $"({nights}n) · {Usd(value)}");

                    switch (b.Status)
                    {
                         case BookingStatus.Pending: pending++; break;
                         case BookingStatus.Confirmed: confirmed++; revenue += value; break;
                         case BookingStatus.Cancelled: cancelled++; break;
                    }
               }

               sb.AppendLine(new string('═', 64));
               sb.AppendLine($"  Pending   : {pending}");
               sb.AppendLine($"  Confirmed : {confirmed}");
               sb.AppendLine($"  Cancelled : {cancelled}");
               sb.AppendLine($"  Revenue   : {Usd(revenue)} (confirmed bookings)");
               return sb.ToString();
          }

          // ── REPORT 2: Occupancy Timeline ─────────────────────────────────────
          // Uses: ChronologicalBookingIterator
          // Yields bookings sorted by check-in — perfect for timeline/Gantt output.
          public string GenerateOccupancyTimeline(IBookingCollection collection,
                                                   IRoomRepository roomRepo)
          {
               var iter = collection.CreateChronologicalIterator();
               var sb = new StringBuilder();

               sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
               sb.AppendLine("║          OCCUPANCY TIMELINE (by check-in date)           ║");
               sb.AppendLine("╚══════════════════════════════════════════════════════════╝");
               sb.AppendLine($"  Iterator  : {iter.IteratorName}");
               sb.AppendLine(new string('─', 64));

               if (!iter.HasNext()) { sb.AppendLine("  No bookings."); return sb.ToString(); }

               while (iter.HasNext())
               {
                    var b = iter.Next();
                    var room = roomRepo.FindById(b.RoomId);
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;

                    // Simple ASCII Gantt bar (1 char per night, max 20 chars)
                    int barLen = Math.Min(nights, 20);
                    string bar = b.Status switch
                    {
                         BookingStatus.Confirmed => new string('█', barLen),
                         BookingStatus.Pending => new string('░', barLen),
                         BookingStatus.Cancelled => new string('×', barLen),
                         _ => new string('·', barLen)
                    };

                    sb.AppendLine(
                        $"  Room {room?.RoomNumber ?? "??",4}  {b.CheckInDate:dd MMM}  " +
                        $"[{bar,-20}]  {b.CheckOutDate:dd MMM}  ({nights}n)  {b.Status}");
               }

               return sb.ToString();
          }

          // ── REPORT 3: Revenue by Booking Type ────────────────────────────────
          // Uses: TypeFilterIterator (called 3 times — Standard, Premium, VIP)
          // Demonstrates multiple INDEPENDENT iterators on the same collection.
          public string GenerateRevenueByTypeReport(IBookingCollection collection,
                                                     IRoomRepository roomRepo)
          {
               var sb = new StringBuilder();
               sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
               sb.AppendLine("║           REVENUE BREAKDOWN BY BOOKING TYPE              ║");
               sb.AppendLine("╚══════════════════════════════════════════════════════════╝");

               decimal grandTotal = 0m;

               foreach (var type in new[] { "Standard", "Premium", "VIP" })
               {
                    // Each call to CreateTypeFilterIterator is a SEPARATE iterator —
                    // independent state, independent cursor, same collection.
                    var iter = collection.CreateTypeFilterIterator(type);
                    decimal sub = 0m;
                    int cnt = 0;

                    sb.AppendLine($"\n  ▸ {type.ToUpper()} bookings  [{iter.TotalCount}]");
                    sb.AppendLine($"  Iterator: {iter.IteratorName}");
                    sb.AppendLine(new string('─', 48));

                    while (iter.HasNext())
                    {
                         var b = iter.Next();
                         var room = roomRepo.FindById(b.RoomId);
                         int nights = (b.CheckOutDate - b.CheckInDate).Days;
                         decimal val = (room?.BasePrice ?? 0m) * nights;
                         if (b.Status != BookingStatus.Cancelled) sub += val;
                         cnt++;
                         sb.AppendLine(
                             $"    {b.BookingId[..8]}… · {b.Status,-10} · " +
                             $"{nights}n · {Usd(val)}");
                    }

                    sb.AppendLine($"  Subtotal ({type}): {Usd(sub)}");
                    grandTotal += sub;
               }

               sb.AppendLine(new string('═', 48));
               sb.AppendLine($"  Grand Total : {Usd(grandTotal)}");
               return sb.ToString();
          }

          // ── REPORT 4: Status Report ───────────────────────────────────────────
          // Uses: StatusFilterIterator — one iterator per status
          public string GenerateStatusReport(IBookingCollection collection,
                                              IRoomRepository roomRepo)
          {
               var sb = new StringBuilder();
               sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
               sb.AppendLine("║                   BOOKINGS BY STATUS                    ║");
               sb.AppendLine("╚══════════════════════════════════════════════════════════╝");

               foreach (var status in Enum.GetValues<BookingStatus>())
               {
                    var iter = collection.CreateStatusFilterIterator(status);
                    if (iter.TotalCount == 0) continue;

                    string marker = status switch
                    {
                         BookingStatus.Confirmed => "✓",
                         BookingStatus.Pending => "⏳",
                         BookingStatus.Cancelled => "✕",
                         BookingStatus.Completed => "★",
                         _ => "·"
                    };

                    sb.AppendLine($"\n  {marker} {status.ToString().ToUpper()} — {iter.TotalCount} booking(s)");
                    sb.AppendLine($"  Iterator: {iter.IteratorName}");

                    decimal total = 0m;
                    while (iter.HasNext())
                    {
                         var b = iter.Next();
                         var room = roomRepo.FindById(b.RoomId);
                         int nights = (b.CheckOutDate - b.CheckInDate).Days;
                         decimal val = (room?.BasePrice ?? 0m) * nights;
                         total += val;
                         sb.AppendLine(
                             $"    {b.BookingId[..8]}… · {b.BookingType,-9} · " +
                             $"{b.CheckInDate:dd MMM} → {b.CheckOutDate:dd MMM} " +
                             $"({nights}n) · {Usd(val)}");
                    }

                    sb.AppendLine($"    Total value: {Usd(total)}");
               }

               return sb.ToString();
          }

          // ── REPORT 5: Recent Bookings (lazy iterator) ─────────────────────────
          // Uses: RecentBookingsIterator — stops after N without scanning full list
          public string GenerateRecentReport(IBookingCollection collection,
                                              IRoomRepository roomRepo,
                                              int count = 10)
          {
               var iter = collection.CreateRecentIterator(count);
               var sb = new StringBuilder();

               sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
               sb.AppendLine($"║          LAST {count} BOOKINGS (newest first, lazy stop)        ║");
               sb.AppendLine("╚══════════════════════════════════════════════════════════╝");
               sb.AppendLine($"  Iterator : {iter.IteratorName}");
               sb.AppendLine(new string('─', 64));

               if (!iter.HasNext()) { sb.AppendLine("  No bookings yet."); return sb.ToString(); }

               int i = 0;
               while (iter.HasNext())
               {
                    var b = iter.Next();
                    var room = roomRepo.FindById(b.RoomId);
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;
                    sb.AppendLine(
                        $"  #{++i,2}  {b.BookingId[..8]}… · {b.BookingType,-9} · " +
                        $"{b.Status,-10} · Room {room?.RoomNumber ?? "?"} · " +
                        $"{b.CheckInDate:dd MMM} ({nights}n)");
               }

               return sb.ToString();
          }

          // ── REPORT 6: Date Range ──────────────────────────────────────────────
          // Uses: DateRangeIterator
          public string GenerateDateRangeReport(IBookingCollection collection,
                                                 IRoomRepository roomRepo,
                                                 DateTime from, DateTime to)
          {
               var iter = collection.CreateDateRangeIterator(from, to);
               var sb = new StringBuilder();

               sb.AppendLine("╔══════════════════════════════════════════════════════════╗");
               sb.AppendLine("║              DATE RANGE OCCUPANCY REPORT                 ║");
               sb.AppendLine("╚══════════════════════════════════════════════════════════╝");
               sb.AppendLine($"  Range    : {from:dd MMM yyyy} – {to:dd MMM yyyy}");
               sb.AppendLine($"  Iterator : {iter.IteratorName}");
               sb.AppendLine($"  Found    : {iter.TotalCount} booking(s) in range");
               sb.AppendLine(new string('─', 64));

               if (!iter.HasNext()) { sb.AppendLine("  No bookings in this date range."); return sb.ToString(); }

               decimal total = 0m;
               while (iter.HasNext())
               {
                    var b = iter.Next();
                    var room = roomRepo.FindById(b.RoomId);
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;
                    decimal val = (room?.BasePrice ?? 0m) * nights;
                    total += val;
                    sb.AppendLine(
                        $"  {b.BookingId[..8]}… · Room {room?.RoomNumber ?? "?",4} · " +
                        $"{b.BookingType,-9} · {b.Status,-10} · {Usd(val)}");
               }

               sb.AppendLine(new string('═', 64));
               sb.AppendLine($"  Total value in range: {Usd(total)}");
               return sb.ToString();
          }
     }
}