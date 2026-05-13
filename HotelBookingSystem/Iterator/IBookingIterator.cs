using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Iterator
{
     // --------------------------------------------------------------------------
     // CE FACE: Aceasta este interfa?a care define?te opera?iile de baza pe care o 
     // colec?ie de rezervari le va vizita. Ea spune CUM citim, dar nu cum sunt datele salvate.
     // DE CE RASPUNDE: Pentru ca decupleaza logica interfe?ei/rapoartelor de lista sau baza     
     // de date care stocheaza rezervarile. Ofera o metoda curata (HasNext(), Next(), Peak()) 
     // astfel încât Clientul (de ex ReportEngine) sa ceara rezervarea urmatoare indiferent 
     // de modul de sortare sau filtrare din spate.
     // --------------------------------------------------------------------------
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

     // --------------------------------------------------------------------------
     // CE FACE: Interfa?a care este aplicata Colec?iei (Agregatului) care stocheaza
     // rezervarile. Define?te "re?etele" specifice pentru tipurile de interare dorite.
     // DE CE RASPUNDE: Declararea metodelor fabrica "Create..Iterator" for?eaza ca
     // colec?ia de rezervari sa nu trimita clientului List<Booking>, ci mereu un Iterator 
     // încastrat (un obiect special care ?tie cum sa parcurga colec?ia, de ex "doar alea confirmate").
     // --------------------------------------------------------------------------
     public interface IBookingCollection
     {
          // -- Standard traversals -----------------------------------------------

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

          // -- C# idiomatic — implements IEnumerable for native foreach ----------
          System.Collections.Generic.IEnumerable<Booking> AsEnumerable();
     }
}

