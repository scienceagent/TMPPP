using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Observer;

namespace HotelBookingSystem.ViewModels
{
     // ── Registered observer row shown in the UI panel ─────────────────────────
     public sealed class ObserverRowViewModel
     {
          public string Name { get; init; } = "";
          public string Description { get; init; } = "";
          public string ColorHex { get; init; } = "#64748B";
          public bool IsActive { get; init; }
     }

     // ── Live KPI card ─────────────────────────────────────────────────────────
     public sealed class KpiCard : BaseViewModel
     {
          private string _value = "0";
          private string _delta = "";

          public string Label { get; init; } = "";
          public string Icon { get; init; } = "";
          public string ColorHex { get; init; } = "#2563EB";

          public string Value
          {
               get => _value;
               set => SetProperty(ref _value, value);
          }
          public string Delta
          {
               get => _delta;
               set => SetProperty(ref _delta, value);
          }
     }

     // ── Main ViewModel ─────────────────────────────────────────────────────────
     public sealed class ObserverController : BaseViewModel
     {
          // ── References to concrete observers (for data binding) ───────────────
          private readonly DashboardObserver _dashboard;
          private readonly OccupancyObserver _occupancy;
          private readonly RevenueObserver _revenue;
          private readonly AlertObserver _alertObs;
          private readonly AuditLogObserver _audit;

          private readonly BookingEventMonitor _monitor;
          private readonly IBookingRepository _bookingRepo;

          // ── Observable collections for UI ─────────────────────────────────────
          public ObservableCollection<ObserverRowViewModel> ObserverRows { get; } = new();
          public ObservableCollection<AuditEntry> AuditEntries { get; } = new();
          public ObservableCollection<AlertEntry> AlertEntries { get; } = new();

          // ── KPI cards ─────────────────────────────────────────────────────────
          public KpiCard KpiBookings { get; } = new() { Label = "Total Bookings", Icon = "📋", ColorHex = "#2563EB" };
          public KpiCard KpiRevenue { get; } = new() { Label = "Pipeline Revenue", Icon = "💰", ColorHex = "#15803D" };
          public KpiCard KpiOccupancy { get; } = new() { Label = "Occupancy Rate", Icon = "🏨", ColorHex = "#0891B2" };
          public KpiCard KpiAlerts { get; } = new() { Label = "Active Alerts", Icon = "🔔", ColorHex = "#DC2626" };

          // ── Occupancy breakdown ────────────────────────────────────────────────
          private string _occupancyBreakdown = "No data yet — create bookings to see live metrics.";
          public string OccupancyBreakdown
          {
               get => _occupancyBreakdown;
               private set => SetProperty(ref _occupancyBreakdown, value);
          }

          // ── Revenue breakdown ──────────────────────────────────────────────────
          private string _revenueBreakdown = "";
          public string RevenueBreakdown
          {
               get => _revenueBreakdown;
               private set => SetProperty(ref _revenueBreakdown, value);
          }

          // ── Dashboard summary ──────────────────────────────────────────────────
          private string _lastActivity = "No activity yet";
          public string LastActivity
          {
               get => _lastActivity;
               private set => SetProperty(ref _lastActivity, value);
          }

          private string _lastActivityTime = "—";
          public string LastActivityTime
          {
               get => _lastActivityTime;
               private set => SetProperty(ref _lastActivityTime, value);
          }

          // ── Commands ──────────────────────────────────────────────────────────
          public ICommand RefreshCommand { get; }

          public event Action<string>? OnLog;

          // ── Constructor ───────────────────────────────────────────────────────
          public ObserverController(BookingEventMonitor monitor, IBookingRepository bookingRepo)
          {
               _monitor = monitor;
               _bookingRepo = bookingRepo;

               // Pull typed references from the monitor's observer list
               // (they were registered in MainViewModel)
               _dashboard = monitor.Observers.OfType<DashboardObserver>().First();
               _occupancy = monitor.Observers.OfType<OccupancyObserver>().First();
               _revenue = monitor.Observers.OfType<RevenueObserver>().First();
               _alertObs = monitor.Observers.OfType<AlertObserver>().First();
               _audit = monitor.Observers.OfType<AuditLogObserver>().First();

               BuildObserverRows();

               RefreshCommand = new RelayCommand(_ => RefreshAll());
          }

          // ── Refresh — called after every booking event ────────────────────────
          public void RefreshAll()
          {
               var en = CultureInfo.GetCultureInfo("en-US");

               // KPI cards
               KpiBookings.Value = _dashboard.TotalCreated.ToString();
               KpiBookings.Delta = $"{_dashboard.TotalConfirmed} confirmed · {_dashboard.TotalCancelled} cancelled";

               KpiRevenue.Value = _revenue.TotalRevenue.ToString("C0", en);
               KpiRevenue.Delta = $"Avg/night: {_revenue.AvgNightlyRate.ToString("C0", en)}";

               KpiOccupancy.Value = $"{_occupancy.OccupancyRate:F0}%";
               KpiOccupancy.Delta = $"{_occupancy.Occupied} in-house · {_occupancy.Reserved} reserved";

               KpiAlerts.Value = _alertObs.AlertCount.ToString();
               KpiAlerts.Delta = _alertObs.AlertCount == 0 ? "All clear" : _alertObs.LastAlert;

               // Occupancy breakdown
               OccupancyBreakdown =
                   $"  Reserved    : {_occupancy.Reserved}\n" +
                   $"  Occupied    : {_occupancy.Occupied}\n" +
                   $"  Cancelled   : {_occupancy.Cancelled}\n" +
                   $"  Available   : {_occupancy.Available}\n" +
                   $"  Rate        : {_occupancy.OccupancyRate:F1}%\n" +
                   $"  Last event  : {_occupancy.LastEntry}";

               // Revenue breakdown
               var rb = _revenue.RevenueByType;
               RevenueBreakdown =
                   $"  Standard    : {rb.GetValueOrDefault("Standard"):C0}\n" +
                   $"  Premium     : {rb.GetValueOrDefault("Premium"):C0}\n" +
                   $"  VIP         : {rb.GetValueOrDefault("VIP"):C0}\n" +
                   $"  Total       : {_revenue.TotalRevenue:C0}\n" +
                   $"  Avg/night   : {_revenue.AvgNightlyRate:C0}\n" +
                   $"  Top type    : {_revenue.TopRevenueType}";

               // Dashboard last activity
               LastActivity = _dashboard.LastActivityDesc;
               LastActivityTime = _dashboard.LastActivityTime;

               // Sync audit log (append only)
               int existing = AuditEntries.Count;
               var all = _audit.Entries;
               for (int i = existing; i < all.Count; i++)
                    AuditEntries.Insert(0, all[i]);   // newest first

               // Sync alerts (append only)
               int existingAlerts = AlertEntries.Count;
               var allAlerts = _alertObs.Alerts;
               for (int i = existingAlerts; i < allAlerts.Count; i++)
                    AlertEntries.Insert(0, allAlerts[i]);

               OnLog?.Invoke("[Observer:Dashboard] Metrics refreshed.");
          }

          private void BuildObserverRows()
          {
               ObserverRows.Clear();
               foreach (var obs in _monitor.Observers)
               {
                    ObserverRows.Add(new ObserverRowViewModel
                    {
                         Name = obs.Name,
                         Description = obs.Description,
                         ColorHex = obs.ColorHex,
                         IsActive = true,
                    });
               }
          }
     }
}