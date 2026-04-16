using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Iterator;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     // ── Row shown in the iterator stats table ─────────────────────────────────
     public sealed class IteratorStatsRow
     {
          public string IteratorName { get; init; } = "";
          public int TotalElements { get; init; }
          public string Filter { get; init; } = "";
          public string Description { get; init; } = "";
          public string ColorHex { get; init; } = "#2563EB";
     }

     // ── Main ViewModel ─────────────────────────────────────────────────────────
     public sealed class IteratorController : BaseViewModel
     {
          // ── Pattern components ─────────────────────────────────────────────────
          private readonly BookingCollection _collection;
          private readonly BookingReportEngine _reportEngine;

          // ── External dependencies ──────────────────────────────────────────────
          private readonly IBookingRepository _bookingRepo;
          private readonly IRoomRepository _roomRepo;
          private readonly IUserRepository _userRepo;

          // ── Report output ──────────────────────────────────────────────────────
          private string _reportOutput = "Select a report type and click Generate.";
          public string ReportOutput
          {
               get => _reportOutput;
               private set => SetProperty(ref _reportOutput, value);
          }

          private string _activeReport = "";
          public string ActiveReport
          {
               get => _activeReport;
               set => SetProperty(ref _activeReport, value);
          }

          // ── Form fields ────────────────────────────────────────────────────────
          private int _recentCount = 5;
          private DateTime _rangeFrom = DateTime.Today.AddDays(-30);
          private DateTime _rangeTo = DateTime.Today.AddDays(60);
          private string _selectedStatus = "Confirmed";
          private string _selectedType = "Standard";

          public int RecentCount
          {
               get => _recentCount;
               set => SetProperty(ref _recentCount, Math.Max(1, value));
          }
          public DateTime RangeFrom
          {
               get => _rangeFrom;
               set => SetProperty(ref _rangeFrom, value);
          }
          public DateTime RangeTo
          {
               get => _rangeTo;
               set => SetProperty(ref _rangeTo, value);
          }
          public string SelectedStatus
          {
               get => _selectedStatus;
               set => SetProperty(ref _selectedStatus, value);
          }
          public string SelectedType
          {
               get => _selectedType;
               set => SetProperty(ref _selectedType, value);
          }

          // ── Live stats ────────────────────────────────────────────────────────
          public ObservableCollection<IteratorStatsRow> IteratorStats { get; } = new();

          public string TotalBookingsDisplay =>
              $"{_collection.TotalCount} total booking(s) in collection";

          // ── Dropdown data ─────────────────────────────────────────────────────
          public IReadOnlyList<string> StatusOptions { get; } =
              new[] { "Pending", "Confirmed", "Cancelled", "Completed" };

          public IReadOnlyList<string> TypeOptions { get; } =
              new[] { "Standard", "Premium", "VIP" };

          public IReadOnlyList<string> ReportOptions { get; } = new[]
          {
            "Summary Report",
            "Occupancy Timeline",
            "Revenue by Type",
            "Status Report",
            "Recent Bookings",
            "Date Range Report",
        };

          // ── Commands ──────────────────────────────────────────────────────────
          public ICommand GenerateSummaryCommand { get; }
          public ICommand GenerateTimelineCommand { get; }
          public ICommand GenerateRevenueCommand { get; }
          public ICommand GenerateStatusCommand { get; }
          public ICommand GenerateRecentCommand { get; }
          public ICommand GenerateDateRangeCommand { get; }
          public ICommand RefreshStatsCommand { get; }

          public event Action<string>? OnLog;

          // ── Constructor ────────────────────────────────────────────────────────
          public IteratorController(
              IBookingRepository bookingRepo,
              IRoomRepository roomRepo,
              IUserRepository userRepo)
          {
               _bookingRepo = bookingRepo;
               _roomRepo = roomRepo;
               _userRepo = userRepo;

               _collection = new BookingCollection(bookingRepo);
               _reportEngine = new BookingReportEngine();

               GenerateSummaryCommand = new RelayCommand(_ => RunSummary());
               GenerateTimelineCommand = new RelayCommand(_ => RunTimeline());
               GenerateRevenueCommand = new RelayCommand(_ => RunRevenue());
               GenerateStatusCommand = new RelayCommand(_ => RunStatus());
               GenerateRecentCommand = new RelayCommand(_ => RunRecent());
               GenerateDateRangeCommand = new RelayCommand(_ => RunDateRange());
               RefreshStatsCommand = new RelayCommand(_ => RefreshStats());

               RefreshStats();
          }

          // ── Report runners ─────────────────────────────────────────────────────

          private void RunSummary()
          {
               ActiveReport = "Summary Report";
               ReportOutput = _reportEngine.GenerateSummaryReport(
                   _collection, _userRepo, _roomRepo);
               OnLog?.Invoke("[Iterator] Generated Summary Report using SequentialBookingIterator");
               RefreshStats();
          }

          private void RunTimeline()
          {
               ActiveReport = "Occupancy Timeline";
               ReportOutput = _reportEngine.GenerateOccupancyTimeline(_collection, _roomRepo);
               OnLog?.Invoke("[Iterator] Generated Occupancy Timeline using ChronologicalBookingIterator");
               RefreshStats();
          }

          private void RunRevenue()
          {
               ActiveReport = "Revenue by Type";
               ReportOutput = _reportEngine.GenerateRevenueByTypeReport(_collection, _roomRepo);
               OnLog?.Invoke("[Iterator] Generated Revenue Report using TypeFilterIterator ×3");
               RefreshStats();
          }

          private void RunStatus()
          {
               ActiveReport = "Status Report";
               ReportOutput = _reportEngine.GenerateStatusReport(_collection, _roomRepo);
               OnLog?.Invoke("[Iterator] Generated Status Report using StatusFilterIterator ×4");
               RefreshStats();
          }

          private void RunRecent()
          {
               ActiveReport = $"Recent {_recentCount} Bookings";
               ReportOutput = _reportEngine.GenerateRecentReport(_collection, _roomRepo, _recentCount);
               OnLog?.Invoke($"[Iterator] Generated Recent Report using RecentBookingsIterator(n={_recentCount})");
               RefreshStats();
          }

          private void RunDateRange()
          {
               ActiveReport = $"Date Range {_rangeFrom:dd MMM} – {_rangeTo:dd MMM yyyy}";
               ReportOutput = _reportEngine.GenerateDateRangeReport(
                   _collection, _roomRepo, _rangeFrom, _rangeTo);
               OnLog?.Invoke($"[Iterator] Generated Date Range Report using DateRangeIterator");
               RefreshStats();
          }

          // ── Refresh live stats panel ───────────────────────────────────────────
          public void RefreshStats()
          {
               OnPropertyChanged(nameof(TotalBookingsDisplay));
               IteratorStats.Clear();

               var all = _collection.TotalCount;

               // Materialise each iterator to show its count in the stats panel
               var stats = new List<IteratorStatsRow>
            {
                Row("Sequential",    all, "All bookings · creation order",           "#2563EB"),
                Row("Chronological", all, "All bookings · sorted by check-in",        "#0891B2"),
                Row("Status: Pending",
                    Count(_collection.CreateStatusFilterIterator(BookingStatus.Pending)),
                    "Awaiting confirmation",                                           "#D97706"),
                Row("Status: Confirmed",
                    Count(_collection.CreateStatusFilterIterator(BookingStatus.Confirmed)),
                    "Confirmed — counts toward revenue",                               "#15803D"),
                Row("Status: Cancelled",
                    Count(_collection.CreateStatusFilterIterator(BookingStatus.Cancelled)),
                    "Cancelled — excluded from revenue",                               "#DC2626"),
                Row($"Type: Standard",
                    Count(_collection.CreateTypeFilterIterator("Standard")),
                    "Standard bookings · base rate",                                   "#6366F1"),
                Row($"Type: Premium",
                    Count(_collection.CreateTypeFilterIterator("Premium")),
                    "Premium bookings · 10% discount",                                 "#7C3AED"),
                Row($"Type: VIP",
                    Count(_collection.CreateTypeFilterIterator("VIP")),
                    "VIP bookings · 20% off + free night",                             "#DB2777"),
                Row($"Date Range",
                    Count(_collection.CreateDateRangeIterator(_rangeFrom, _rangeTo)),
                    $"{_rangeFrom:dd MMM} – {_rangeTo:dd MMM}",                        "#059669"),
                Row($"Recent 5",
                    Count(_collection.CreateRecentIterator(5)),
                    "Last 5 · lazy stop",                                              "#F59E0B"),
            };

               foreach (var s in stats)
                    IteratorStats.Add(s);
          }

          private static IteratorStatsRow Row(string name, int count, string desc, string color) =>
              new() { IteratorName = name, TotalElements = count, Description = desc, ColorHex = color };

          private static int Count(IBookingIterator iter)
          {
               // Materialise to count — just read TotalCount (already computed)
               return iter.TotalCount;
          }
     }
}