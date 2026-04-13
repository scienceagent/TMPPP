using System;
using System.Globalization;
using System.Text;

namespace HotelBookingSystem.Strategy
{
     // ─── shared helper ────────────────────────────────────────────────────────
     file static class F
     {
          internal static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");
          internal static string Usd(decimal v) => v.ToString("C", En);
     }

     // ══════════════════════════════════════════════════════════════════════════
     // STRATEGY 1 — Standard Rate
     // Base price × nights. No adjustments. Baseline for all comparisons.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class StandardRateStrategy : IRoomPricingStrategy
     {
          public string Name => "Standard Rate";
          public string Description => "Base price × nights — no adjustments. Baseline reference rate.";
          public string ColorHex => "#2E9CCA";

          public PricingResult Calculate(decimal basePrice, DateTime checkIn, DateTime checkOut)
          {
               int nights = Math.Max(1, (checkOut - checkIn).Days);
               decimal total = basePrice * nights;

               var sb = new StringBuilder();
               sb.AppendLine($"  Base price   : {F.Usd(basePrice)} / night");
               sb.AppendLine($"  Nights       : {nights}");
               sb.AppendLine($"  Calculation  : {F.Usd(basePrice)} × {nights} = {F.Usd(total)}");
               sb.AppendLine($"  Adjustment   : none");
               sb.AppendLine($"  ─────────────────────────────────────────");
               sb.AppendLine($"  Total        : {F.Usd(total)}");

               return new PricingResult(Name, total, total, nights, sb.ToString());
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // STRATEGY 2 — Weekend Surge
     // Weekday (Sun-Thu) nights at base price.
     // Friday and Saturday nights at 1.5× (peak demand surcharge).
     // Calculated per-night so mixed-week stays are accurate.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class WeekendSurgeStrategy : IRoomPricingStrategy
     {
          private const decimal SurgeMultiplier = 1.5m;

          public string Name => "Weekend Surge";
          public string Description => "Weekday nights at base · Friday & Saturday nights at 1.5× (peak demand).";
          public string ColorHex => "#F59E0B";

          public PricingResult Calculate(decimal basePrice, DateTime checkIn, DateTime checkOut)
          {
               int nights = Math.Max(1, (checkOut - checkIn).Days);
               decimal total = 0m;
               int weekendNights = 0;
               int weekdayNights = 0;

               for (int i = 0; i < nights; i++)
               {
                    var night = checkIn.AddDays(i);
                    bool isWeekend = night.DayOfWeek == DayOfWeek.Friday ||
                                     night.DayOfWeek == DayOfWeek.Saturday;
                    if (isWeekend) { total += basePrice * SurgeMultiplier; weekendNights++; }
                    else { total += basePrice; weekdayNights++; }
               }

               decimal baseTotal = basePrice * nights;

               var sb = new StringBuilder();
               sb.AppendLine($"  Base price    : {F.Usd(basePrice)} / night");
               sb.AppendLine($"  Weekday nights: {weekdayNights} × {F.Usd(basePrice)} = {F.Usd(basePrice * weekdayNights)}");
               sb.AppendLine($"  Weekend nights: {weekendNights} × {F.Usd(basePrice * SurgeMultiplier)} (×{SurgeMultiplier} surge) = {F.Usd(basePrice * SurgeMultiplier * weekendNights)}");
               if (weekendNights > 0)
                    sb.AppendLine($"  Surcharge     : +{F.Usd(total - baseTotal)} (+{(total - baseTotal) / baseTotal * 100:F1}% vs base)");
               sb.AppendLine($"  ─────────────────────────────────────────");
               sb.AppendLine($"  Total         : {F.Usd(total)}");

               return new PricingResult(Name, baseTotal, total, nights, sb.ToString());
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // STRATEGY 3 — Early Bird Discount
     // Rewards guests who book well in advance.
     //   30+ days ahead  →  20% off the full stay
     //   14-29 days      →  10% off
     //   < 14 days       →  no discount
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class EarlyBirdStrategy : IRoomPricingStrategy
     {
          public string Name => "Early Bird";
          public string Description => "Book 30+ days ahead: 20% off · 14-29 days: 10% off · Under 14 days: no discount.";
          public string ColorHex => "#10B981";

          public PricingResult Calculate(decimal basePrice, DateTime checkIn, DateTime checkOut)
          {
               int nights = Math.Max(1, (checkOut - checkIn).Days);
               int daysAhead = Math.Max(0, (checkIn - DateTime.Today).Days);
               decimal baseTotal = basePrice * nights;

               decimal discountRate;
               string tier;

               if (daysAhead >= 30) { discountRate = 0.20m; tier = $"{daysAhead} days ahead → 20% Early Bird discount"; }
               else if (daysAhead >= 14) { discountRate = 0.10m; tier = $"{daysAhead} days ahead → 10% Early Bird discount"; }
               else { discountRate = 0.00m; tier = $"{daysAhead} days ahead → no early-bird discount (need 14+ days)"; }

               decimal discount = baseTotal * discountRate;
               decimal total = baseTotal - discount;

               var sb = new StringBuilder();
               sb.AppendLine($"  Base price    : {F.Usd(basePrice)} / night × {nights} nights = {F.Usd(baseTotal)}");
               sb.AppendLine($"  Booking date  : {DateTime.Today:dd MMM yyyy}");
               sb.AppendLine($"  Check-in date : {checkIn:dd MMM yyyy}");
               sb.AppendLine($"  Tier          : {tier}");
               if (discountRate > 0)
                    sb.AppendLine($"  Discount      : -{F.Usd(discount)} (-{discountRate * 100:F0}%)");
               sb.AppendLine($"  ─────────────────────────────────────────");
               sb.AppendLine($"  Total         : {F.Usd(total)}");

               return new PricingResult(Name, baseTotal, total, nights, sb.ToString());
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // STRATEGY 4 — Last Minute Deal
     // Incentivises late bookings to fill otherwise empty rooms.
     //   0-3 days before  →  25% off
     //   4-7 days         →  15% off
     //   8+ days          →  no last-minute deal
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class LastMinuteDealStrategy : IRoomPricingStrategy
     {
          public string Name => "Last Minute Deal";
          public string Description => "0-3 days before: 25% off · 4-7 days: 15% off · 8+ days: no deal.";
          public string ColorHex => "#EF4444";

          public PricingResult Calculate(decimal basePrice, DateTime checkIn, DateTime checkOut)
          {
               int nights = Math.Max(1, (checkOut - checkIn).Days);
               int daysAhead = Math.Max(0, (checkIn - DateTime.Today).Days);
               decimal baseTotal = basePrice * nights;

               decimal discountRate;
               string tier;

               if (daysAhead <= 3) { discountRate = 0.25m; tier = $"{daysAhead} day(s) away → 25% Last Minute deal"; }
               else if (daysAhead <= 7) { discountRate = 0.15m; tier = $"{daysAhead} days away → 15% Last Minute deal"; }
               else { discountRate = 0.00m; tier = $"{daysAhead} days away → no last-minute deal (need ≤ 7 days)"; }

               decimal discount = baseTotal * discountRate;
               decimal total = baseTotal - discount;

               var sb = new StringBuilder();
               sb.AppendLine($"  Base price    : {F.Usd(basePrice)} / night × {nights} nights = {F.Usd(baseTotal)}");
               sb.AppendLine($"  Days to check-in: {daysAhead}");
               sb.AppendLine($"  Tier          : {tier}");
               if (discountRate > 0)
                    sb.AppendLine($"  Discount      : -{F.Usd(discount)} (-{discountRate * 100:F0}%)");
               sb.AppendLine($"  ─────────────────────────────────────────");
               sb.AppendLine($"  Total         : {F.Usd(total)}");

               return new PricingResult(Name, baseTotal, total, nights, sb.ToString());
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // STRATEGY 5 — Long Stay Discount
     // Rewards extended stays with progressive discounts.
     //   1-6 nights   →  0%
     //   7-13 nights  →  5%
     //   14-20 nights →  10%
     //   21-27 nights →  15%
     //   28+ nights   →  20% (maximum)
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class LongStayStrategy : IRoomPricingStrategy
     {
          public string Name => "Long Stay Discount";
          public string Description => "7+ nights: 5% · 14+ nights: 10% · 21+ nights: 15% · 28+ nights: 20% off.";
          public string ColorHex => "#8B5CF6";

          public PricingResult Calculate(decimal basePrice, DateTime checkIn, DateTime checkOut)
          {
               int nights = Math.Max(1, (checkOut - checkIn).Days);
               decimal baseTotal = basePrice * nights;

               decimal discountRate;
               string tier;

               if (nights >= 28) { discountRate = 0.20m; tier = $"{nights} nights → 20% Long Stay discount"; }
               else if (nights >= 21) { discountRate = 0.15m; tier = $"{nights} nights → 15% Long Stay discount"; }
               else if (nights >= 14) { discountRate = 0.10m; tier = $"{nights} nights → 10% Long Stay discount"; }
               else if (nights >= 7) { discountRate = 0.05m; tier = $"{nights} nights → 5% Long Stay discount"; }
               else { discountRate = 0.00m; tier = $"{nights} nights → no discount (need 7+ nights)"; }

               decimal discount = baseTotal * discountRate;
               decimal total = baseTotal - discount;

               var sb = new StringBuilder();
               sb.AppendLine($"  Base price    : {F.Usd(basePrice)} / night × {nights} nights = {F.Usd(baseTotal)}");
               sb.AppendLine($"  Length tier   : {tier}");
               if (discountRate > 0)
                    sb.AppendLine($"  Discount      : -{F.Usd(discount)} (-{discountRate * 100:F0}%)");
               sb.AppendLine($"  ─────────────────────────────────────────");
               sb.AppendLine($"  Total         : {F.Usd(total)}");

               return new PricingResult(Name, baseTotal, total, nights, sb.ToString());
          }
     }

     // ══════════════════════════════════════════════════════════════════════════
     // STRATEGY 6 — Seasonal Rate
     // Per-night multiplier based on which season each night falls in.
     //   Jun-Aug (Summer peak) :  +40%
     //   Dec-Feb (Winter peak) :  +30%
     //   Mar-May (Spring)      :  +5%
     //   Sep-Nov (Autumn)      :  standard
     // Mixed-season stays are calculated night-by-night.
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class SeasonalRateStrategy : IRoomPricingStrategy
     {
          public string Name => "Seasonal Rate";
          public string Description => "Summer peak: +40% · Winter peak: +30% · Spring: +5% · Autumn: standard. Per-night.";
          public string ColorHex => "#F97316";

          private static (decimal Multiplier, string Season) GetSeason(DateTime date) =>
              date.Month switch
              {
                   12 or 1 or 2 => (1.30m, "Winter peak  (+30%)"),
                   3 or 4 or 5 => (1.05m, "Spring       (+5%)"),
                   6 or 7 or 8 => (1.40m, "Summer peak  (+40%)"),
                   _ => (1.00m, "Autumn       (standard)")
              };

          public PricingResult Calculate(decimal basePrice, DateTime checkIn, DateTime checkOut)
          {
               int nights = Math.Max(1, (checkOut - checkIn).Days);
               decimal total = 0m;
               decimal baseTotal = basePrice * nights;

               // Track seasons for the breakdown
               string? lastSeason = null;
               int seasonCount = 0;
               decimal seasonCost = 0m;
               var sb = new StringBuilder();
               sb.AppendLine($"  Base price    : {F.Usd(basePrice)} / night");
               sb.AppendLine($"  Stay period   : {checkIn:dd MMM yyyy} → {checkOut:dd MMM yyyy}  ({nights} nights)");
               sb.AppendLine($"  Per-night breakdown:");

               void FlushSeason()
               {
                    if (lastSeason != null && seasonCount > 0)
                         sb.AppendLine($"    {lastSeason}: {seasonCount} night(s) → {F.Usd(seasonCost)}");
               }

               for (int i = 0; i < nights; i++)
               {
                    var night = checkIn.AddDays(i);
                    var (mult, season) = GetSeason(night);
                    decimal rate = basePrice * mult;

                    if (season != lastSeason)
                    {
                         FlushSeason();
                         lastSeason = season;
                         seasonCount = 0;
                         seasonCost = 0m;
                    }
                    seasonCount++;
                    seasonCost += rate;
                    total += rate;
               }
               FlushSeason();

               if (total > baseTotal)
                    sb.AppendLine($"  Peak surcharge: +{F.Usd(total - baseTotal)} (+{(total - baseTotal) / baseTotal * 100:F1}% vs base)");
               sb.AppendLine($"  ─────────────────────────────────────────");
               sb.AppendLine($"  Total         : {F.Usd(total)}");

               return new PricingResult(Name, baseTotal, total, nights, sb.ToString());
          }
     }
}