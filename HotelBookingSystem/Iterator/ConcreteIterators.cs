using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Iterator
{
     // --------------------------------------------------------------------------
     // CE FACE / DE CE R?SPUNDE: O clas? de baz? (BookingIteratorBase) folositoare pentru  
     // cei mai mul?i iteratori având variabile pentru stocarea pozi?iei de vizitare ('_position') 
     // ?i liste temporare (_items). Reduce vizibil buc??i identice de cod pentru clasele derivate.
     // --------------------------------------------------------------------------
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

     // --------------------------------------------------------------------------
     // ITERATOR 1  Sequential (insertion order)
     // CE FACE: Returnez? lista exact a?a cum vine f?r? nici o procesare pe ea.
     // DE CE R?SPUNDE: Pentru rapoarte standard ce urmeaz? o simpl? preluare istoric? nativ?.
     // --------------------------------------------------------------------------
     public class SequentialBookingIterator : BookingIteratorBase
     {
          public SequentialBookingIterator(IReadOnlyList<Booking> items)
              : base(items) { }

          public override string IteratorName => "Sequential (creation order)";
     }

     // --------------------------------------------------------------------------
     // ITERATOR 2  Chronological (sorted by check-in date)
     // CE FACE: La creare ordoneaz? colec?ia de surs? dup? data de intrare al oaspe?ilor.
     // DE CE R?SPUNDE: F?r? a modifica clasa BookingCollection de re?ete, rezolv?m  
     // cu u?urin?? afi?area unui raport al cronologiei ocup?rilor.
     // --------------------------------------------------------------------------
     public class ChronologicalBookingIterator : BookingIteratorBase
     {
          public ChronologicalBookingIterator(IEnumerable<Booking> source)
              : base(source.OrderBy(b => b.CheckInDate).ToList()) { }

          public override string IteratorName => "Chronological (check-in date ASC)";
     }

     // --------------------------------------------------------------------------
     // ITERATOR 3  Status Filter
     // CE FACE: Filtreaz? înainte de a începe vizitarea (ex. doar Confirmed).
     // DE CE R?SPUNDE: Clientul cere un Iterator cu stare dat? ?i când folose?te HasNext(), 
     // deja g?se?te doar elementele cu acea stare fâra a mai face if(Status == ...) a doua oar?.
     // --------------------------------------------------------------------------
     public class StatusFilterIterator : IBookingIterator
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

     // --------------------------------------------------------------------------
     // ITERATOR 4  Date Range Filter
     // CE FACE: Taie ?i sorteaz? din list? acele rezerveuri aflate strict in [start, end].
     // DE CE R?SPUNDE: Extinde simplu interac?iunea pattern-ului oferind ?i alte variabile de selec?ie independente.
     // --------------------------------------------------------------------------
     public class DateRangeIterator : BookingIteratorBase
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

     // --------------------------------------------------------------------------
     // ITERATOR 5  Recent N Bookings (lazy stop)
     // CE FACE: Intoarce în ordinea descresc?toare de creare cu o limita MAXIMAL? (Take(count)).
     // DE CE R?SPUNDE: Optimizeaz? memoria ?i durata de parcurgere la func?iile "Ultimele X rezerv?ri ad?ugate",  
     // unde x este doar o felie cerut?.
     // --------------------------------------------------------------------------
     public class RecentBookingsIterator : IBookingIterator
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

     // --------------------------------------------------------------------------
     // ITERATOR 6  Booking Type Filter
     // CE FACE: Aduce elementele având un singur rol special - strict doar cele din filtrul Type (Standard, VIP).
     // DE CE R?SPUNDE: Ofer? raportului de tip `Revenue by type` o implementare foarte curat? aplicând interatori concomiten?i.
     // --------------------------------------------------------------------------
     public class TypeFilterIterator : BookingIteratorBase
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

