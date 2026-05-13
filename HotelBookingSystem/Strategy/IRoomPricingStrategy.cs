using System;

namespace HotelBookingSystem.Strategy
{
     // --------------------------------------------------------------------------
     // Interfa?a comun? pentru to?i algoritmii de calcul al pre?urilor.
     // Define?te metoda de baz? Calculate(...) ?i atribute vizuale (Name, Color).
     // Permite înlocuirea instantanee a oric?rei strategii de pre?uri f?r? s?
     // afecteze structura calculatorului principal (Context).
     // --------------------------------------------------------------------------
     public interface IRoomPricingStrategy
     {
          /// <summary>Human-readable name shown in the UI selector and comparison table.</summary>
          string Name { get; }

          /// <summary>One-line explanation of the algorithm shown as a tooltip / description.</summary>
          string Description { get; }

          /// <summary>Accent hex colour used to identify this strategy in the comparison grid.</summary>
          string ColorHex { get; }

          /// <summary>
          /// Calculates the total price for a stay.
          /// All logic lives HERE — Context contains zero pricing logic.
          /// </summary>
          PricingResult Calculate(decimal basePrice, DateTime checkIn, DateTime checkOut);
     }

     // --------------------------------------------------------------------------
     // O structur?/obiect stabil (imutabil) folosit pentru afi?ajul detaliat al rezultatelor.
     // Re?ine date ob?inute de la Strategia activ? f?r? s? ?tie care este ea exact (rezumatul UI).
     // --------------------------------------------------------------------------
     public class PricingResult
     {
          public string StrategyName { get; }
          public decimal BaseTotal { get; }   // basePrice × nights, no modifiers
          public decimal FinalTotal { get; }   // after all adjustments
          public decimal Discount { get; }   // positive = saving; negative = surcharge
          public decimal DiscountPercent { get; }   // percentage vs base
          public int Nights { get; }
          public decimal EffectiveNightlyRate { get; }   // FinalTotal / Nights
          public string Breakdown { get; }   // multi-line step-by-step log

          public PricingResult(string strategyName, decimal baseTotal,
                                decimal finalTotal, int nights, string breakdown)
          {
               StrategyName = strategyName;
               BaseTotal = baseTotal;
               FinalTotal = finalTotal;
               Discount = baseTotal - finalTotal;   // positive = saved money
               DiscountPercent = baseTotal == 0 ? 0 :
                                      (baseTotal - finalTotal) / baseTotal * 100m;
               Nights = nights;
               EffectiveNightlyRate = nights > 0 ? finalTotal / nights : 0m;
               Breakdown = breakdown;
          }
     }
}


