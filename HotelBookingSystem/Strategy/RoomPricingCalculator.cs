using System;
using System.Collections.Generic;

namespace HotelBookingSystem.Strategy
{
     // ══════════════════════════════════════════════════════════════════════════
     // CONTEXT
     // RoomPricingCalculator holds a reference to the current IRoomPricingStrategy.
     // It delegates ALL pricing logic to the strategy — the Context itself
     // contains ZERO pricing calculations. That is the defining characteristic
     // of the Strategy pattern: the algorithm lives in the strategy, not here.
     //
     // The strategy can be swapped at runtime via SetStrategy() with zero changes
     // to this class or to any code that calls CalculatePrice().
     //
     // This is separate from the existing IRoomPricingService (Labs 3-4) which
     // handles room-service surcharges and cleaning costs.
     // RoomPricingCalculator is a front-desk pricing tool used during booking.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class RoomPricingCalculator
     {
          // ── Current strategy — the only mutable state ─────────────────────────
          private IRoomPricingStrategy _strategy;

          public IRoomPricingStrategy CurrentStrategy => _strategy;

          public RoomPricingCalculator(IRoomPricingStrategy initialStrategy)
          {
               _strategy = initialStrategy
                           ?? throw new ArgumentNullException(nameof(initialStrategy));
          }

          // ── Swap strategy at runtime — zero other code changes needed ──────────
          public void SetStrategy(IRoomPricingStrategy strategy)
          {
               _strategy = strategy
                           ?? throw new ArgumentNullException(nameof(strategy));
          }

          // ── PRIMARY OPERATION — delegates entirely to the strategy ─────────────
          public PricingResult CalculatePrice(decimal basePrice,
                                               DateTime checkIn,
                                               DateTime checkOut)
          {
               if (basePrice <= 0)
                    throw new ArgumentException("Base price must be positive.", nameof(basePrice));
               if (checkOut <= checkIn)
                    throw new ArgumentException("Check-out must be after check-in.", nameof(checkOut));

               // ← ALL pricing logic lives in _strategy, not here
               return _strategy.Calculate(basePrice, checkIn, checkOut);
          }

          // ── COMPARISON — run all strategies on the same input, sorted cheapest first ──
          // Used by the UI to show the full comparison table without making
          // the ViewModel call Calculate() individually.
          public IReadOnlyList<PricingResult> CompareAllStrategies(
              decimal basePrice,
              DateTime checkIn,
              DateTime checkOut,
              IEnumerable<IRoomPricingStrategy> strategies)
          {
               var results = new List<PricingResult>();
               foreach (var s in strategies)
               {
                    try { results.Add(s.Calculate(basePrice, checkIn, checkOut)); }
                    catch { /* skip degenerate edge cases */ }
               }
               results.Sort((a, b) => a.FinalTotal.CompareTo(b.FinalTotal));
               return results;
          }
     }
}