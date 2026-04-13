using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HotelBookingSystem.Strategy;

namespace HotelBookingSystem.ViewModels
{
     // ── Comparison table row ───────────────────────────────────────────────────
     public sealed class StrategyComparisonRow
     {
          private static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");

          public string StrategyName { get; init; } = "";
          public string ColorHex { get; init; } = "#64748B";
          public string Description { get; init; } = "";
          public decimal FinalTotal { get; init; }
          public decimal Discount { get; init; }   // positive = saving; negative = surcharge
          public decimal DiscountPercent { get; init; }
          public decimal EffectiveRate { get; init; }
          public int Nights { get; init; }
          public bool IsCheapest { get; init; }
          public bool IsMostExpensive { get; init; }
          public bool IsSelected { get; init; }

          // Formatted display strings
          public string FinalTotalFmt => FinalTotal.ToString("C", En);
          public string EffectiveRateFmt => EffectiveRate.ToString("C", En) + " / night";
          public string DiscountFmt
          {
               get
               {
                    if (Discount > 0) return $"-{Discount.ToString("C", En)}  (-{DiscountPercent:F1}%)";
                    if (Discount < 0) return $"+{(-Discount).ToString("C", En)}  (+{-DiscountPercent:F1}%)";
                    return "—";
               }
          }
          public string BadgeText => IsCheapest ? "BEST" : IsMostExpensive ? "HIGH" : "";
     }

     // ── Main ViewModel ─────────────────────────────────────────────────────────
     public sealed class StrategyController : BaseViewModel
     {
          // ── All available strategies ──────────────────────────────────────────
          private readonly List<IRoomPricingStrategy> _allStrategies;

          // ── Context (holds the active strategy) ───────────────────────────────
          private readonly RoomPricingCalculator _calculator;

          // ── Form inputs ───────────────────────────────────────────────────────
          private decimal _basePrice = 150m;
          private DateTime _checkIn = DateTime.Today.AddDays(35);
          private DateTime _checkOut = DateTime.Today.AddDays(37);
          private string _selectedStrategyName = "Standard Rate";

          // ── Outputs ───────────────────────────────────────────────────────────
          private string _breakdownOutput = "Select a strategy and dates above to calculate.";
          private string _currentResult = "";
          private string _bestOptionNote = "";
          private string _currentStrategyColor = "#2E9CCA";

          // ── Properties ────────────────────────────────────────────────────────

          public decimal BasePrice
          {
               get => _basePrice;
               set { if (SetProperty(ref _basePrice, value)) RunCalculation(); }
          }

          public DateTime CheckIn
          {
               get => _checkIn;
               set { if (SetProperty(ref _checkIn, value)) RunCalculation(); }
          }

          public DateTime CheckOut
          {
               get => _checkOut;
               set { if (SetProperty(ref _checkOut, value)) RunCalculation(); }
          }

          public int Nights => CheckOut > CheckIn ? (CheckOut - CheckIn).Days : 0;

          public string SelectedStrategyName
          {
               get => _selectedStrategyName;
               set
               {
                    if (!SetProperty(ref _selectedStrategyName, value)) return;
                    // ── RUNTIME SWAP — core of the Strategy pattern ──────────────
                    var strategy = _allStrategies.FirstOrDefault(s => s.Name == value);
                    if (strategy != null)
                         _calculator.SetStrategy(strategy);
                    RunCalculation();
                    CurrentStrategyColor = strategy?.ColorHex ?? "#2E9CCA";
               }
          }

          public string CurrentStrategyColor
          {
               get => _currentStrategyColor;
               private set => SetProperty(ref _currentStrategyColor, value);
          }

          public string BreakdownOutput
          {
               get => _breakdownOutput;
               private set => SetProperty(ref _breakdownOutput, value);
          }

          public string CurrentResult
          {
               get => _currentResult;
               private set => SetProperty(ref _currentResult, value);
          }

          public string BestOptionNote
          {
               get => _bestOptionNote;
               private set => SetProperty(ref _bestOptionNote, value);
          }

          // ── Observable collections ────────────────────────────────────────────
          public ObservableCollection<string> StrategyNames { get; } = new();
          public ObservableCollection<StrategyComparisonRow> ComparisonRows { get; } = new();

          public event Action<string>? OnLog;

          // ── Constructor ───────────────────────────────────────────────────────
          public StrategyController()
          {
               _allStrategies = new List<IRoomPricingStrategy>
            {
                new StandardRateStrategy(),
                new WeekendSurgeStrategy(),
                new EarlyBirdStrategy(),
                new LastMinuteDealStrategy(),
                new LongStayStrategy(),
                new SeasonalRateStrategy(),
            };

               foreach (var s in _allStrategies)
                    StrategyNames.Add(s.Name);

               _calculator = new RoomPricingCalculator(_allStrategies[0]);

               RunCalculation();
          }

          // ── Core calculation ──────────────────────────────────────────────────
          public void RunCalculation()
          {
               if (_basePrice <= 0 || _checkOut <= _checkIn)
               {
                    BreakdownOutput = "⚠  Enter a valid base price (> 0) and date range.";
                    return;
               }

               OnPropertyChanged(nameof(Nights));

               // Run the currently selected strategy via the Context
               PricingResult selected;
               try { selected = _calculator.CalculatePrice(_basePrice, _checkIn, _checkOut); }
               catch (Exception ex) { BreakdownOutput = $"Error: {ex.Message}"; return; }

               static string Usd(decimal v) =>
                   v.ToString("C", CultureInfo.GetCultureInfo("en-US"));

               // Update breakdown terminal
               BreakdownOutput = selected.Breakdown;

               // Build the result summary line
               string delta = selected.Discount > 0
                   ? $"save  {Usd(selected.Discount)}  ({selected.DiscountPercent:F1}% off)"
                   : selected.Discount < 0
                       ? $"surcharge  +{Usd(-selected.Discount)}  (+{-selected.DiscountPercent:F1}%)"
                       : "no adjustment";

               CurrentResult =
                   $"Total:  {Usd(selected.FinalTotal)}   ·   {selected.Nights} night(s)" +
                   $"   ·   {Usd(selected.EffectiveNightlyRate)} / night avg   ·   {delta}";

               OnLog?.Invoke(
                   $"[Strategy] '{selected.StrategyName}' → " +
                   $"{Usd(selected.FinalTotal)}  ({selected.Nights} nights @ {Usd(selected.EffectiveNightlyRate)}/night)");

               // Run all strategies for the comparison table
               var allResults = _calculator.CompareAllStrategies(
                   _basePrice, _checkIn, _checkOut, _allStrategies);

               var cheapest = allResults.Count > 0 ? allResults[0] : null;
               var mostExp = allResults.Count > 1 ? allResults[^1] : null;

               ComparisonRows.Clear();
               foreach (var r in allResults)
               {
                    var strategy = _allStrategies.First(s => s.Name == r.StrategyName);
                    ComparisonRows.Add(new StrategyComparisonRow
                    {
                         StrategyName = r.StrategyName,
                         ColorHex = strategy.ColorHex,
                         Description = strategy.Description,
                         FinalTotal = r.FinalTotal,
                         Discount = r.Discount,
                         DiscountPercent = r.DiscountPercent,
                         EffectiveRate = r.EffectiveNightlyRate,
                         Nights = r.Nights,
                         IsCheapest = r.StrategyName == cheapest?.StrategyName,
                         IsMostExpensive = r.StrategyName == mostExp?.StrategyName && allResults.Count > 1,
                         IsSelected = r.StrategyName == _selectedStrategyName,
                    });
               }

               BestOptionNote = cheapest != null && cheapest.StrategyName != _selectedStrategyName
                   ? $"💡  Best deal for these dates: \"{cheapest.StrategyName}\" — {Usd(cheapest.FinalTotal)}" +
                     $"  (saves {Usd(selected.FinalTotal - cheapest.FinalTotal)} vs your current selection)"
                   : cheapest?.StrategyName == _selectedStrategyName
                       ? "✓  You have selected the best deal available for these dates."
                       : "";
          }
     }
}