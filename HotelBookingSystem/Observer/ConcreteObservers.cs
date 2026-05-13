using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HotelBookingSystem.Observer
{
     // --- shared helper --------------------------------------------------------
     file static class F
     {
          internal static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");
          internal static string Usd(decimal v) => v.ToString("C", En);
          internal static string Ts(DateTime d) => d.ToString("HH:mm:ss", En);
     }

     // --------------------------------------------------------------------------
     // OBSERVER 1  Occupancy Observer
     // Contorizeaza strict gradul de ocupare al hotelului verificand procentajul "rezervat/ocupat".
     // Cand cineva creeaza o rezervare, stie automat ca trebuie sa puna pe '+1' numarul camerelor rezervate.
     // --------------------------------------------------------------------------
     public class OccupancyObserver : IBookingObserver
     {
          public string Name => "Occupancy Monitor";
          public string Description => "Tracks room states: Available / Reserved / Occupied. Calculates live occupancy %.";
          public string ColorHex => "#0369A1";

          // Live state
          private int _reserved = 0;
          private int _occupied = 0;
          private int _cancelled = 0;
          
          private readonly Func<int> _getTotalRooms;

          public OccupancyObserver(Func<int> getTotalRooms)
          {
               _getTotalRooms = getTotalRooms;
          }

          public int Reserved => _reserved;
          public int Occupied => _occupied;
          public int Cancelled => _cancelled;

          public int Available =>
              Math.Max(0, _getTotalRooms() - _reserved - _occupied);

          public decimal OccupancyRate =>
              _getTotalRooms() == 0 ? 0m :
              (decimal)(_reserved + _occupied) / _getTotalRooms() * 100m;

          public string LastEntry { get; private set; } = "Waiting for events…";

          public void OnBookingEvent(BookingEvent evt)
          {

               switch (evt.EventType)
               {
                    case BookingEventType.BookingCreated:
                    case BookingEventType.BookingConfirmed:
                         _reserved++;
                         LastEntry = $"{F.Ts(evt.OccurredAt)} Room {evt.RoomNumber} ? RESERVED";
                         break;

                    case BookingEventType.GuestCheckedIn:
                         if (_reserved > 0) _reserved--;
                         _occupied++;
                         LastEntry = $"{F.Ts(evt.OccurredAt)} Room {evt.RoomNumber} ? OCCUPIED";
                         break;

                    case BookingEventType.GuestCheckedOut:
                         if (_occupied > 0) _occupied--;
                         LastEntry = $"{F.Ts(evt.OccurredAt)} Room {evt.RoomNumber} ? AVAILABLE";
                         break;

                    case BookingEventType.BookingCancelled:
                         if (_reserved > 0) _reserved--;
                         _cancelled++;
                         LastEntry = $"{F.Ts(evt.OccurredAt)} Room {evt.RoomNumber} booking CANCELLED";
                         break;
               }
          }
     }

     // --------------------------------------------------------------------------
     // OBSERVER 2  Revenue Observer
     // Contorizeaza strict logica banilor rulati per total in hotel de la distanta.
     // Extrage calculul global prin insumarea preturilor si anularea lor. Doar "contabilitate".
     // --------------------------------------------------------------------------
     public class RevenueObserver : IBookingObserver
     {
          public string Name => "Revenue Tracker";
          public string Description => "Accumulates booking revenue by type. Calculates nightly average and total pipeline.";
          public string ColorHex => "#15803D";

          private decimal _totalRevenue = 0m;
          private decimal _confirmedRevenue = 0m;
          private decimal _cancelledRevenue = 0m;
          private int _totalBookings = 0;
          private int _totalNights = 0;

          private readonly Dictionary<string, decimal> _revenueByType = new()
          {
               ["Standard"] = 0m,
               ["Premium"] = 0m,
               ["VIP"] = 0m,
          };

          public decimal TotalRevenue => _totalRevenue;
          public decimal ConfirmedRevenue => _confirmedRevenue;
          public decimal CancelledRevenue => _cancelledRevenue;
          public decimal AvgNightlyRate => _totalNights == 0 ? 0 : _totalRevenue / _totalNights;
          public int TotalBookings => _totalBookings;
          public string TopRevenueType =>
              _revenueByType.OrderByDescending(kv => kv.Value).First().Key;

          public IReadOnlyDictionary<string, decimal> RevenueByType => _revenueByType;

          public string LastEntry { get; private set; } = "Waiting for events…";

          public void OnBookingEvent(BookingEvent evt)
          {
               switch (evt.EventType)
               {
                    case BookingEventType.BookingCreated:
                         _totalRevenue += evt.TotalValue;
                         _totalBookings++;
                         _totalNights += evt.Nights;
                         if (_revenueByType.ContainsKey(evt.BookingType))
                              _revenueByType[evt.BookingType] += evt.TotalValue;
                         LastEntry = $"{F.Ts(evt.OccurredAt)} +{F.Usd(evt.TotalValue)} ({evt.BookingType})";
                         break;

                    case BookingEventType.BookingConfirmed:
                         _confirmedRevenue += evt.TotalValue;
                         LastEntry = $"{F.Ts(evt.OccurredAt)} Confirmed {F.Usd(evt.TotalValue)}";
                         break;

                    case BookingEventType.BookingCancelled:
                         _cancelledRevenue += evt.TotalValue;
                         _totalRevenue -= evt.TotalValue;
                         if (_totalNights >= evt.Nights) _totalNights -= evt.Nights;
                         if (_revenueByType.ContainsKey(evt.BookingType))
                              _revenueByType[evt.BookingType] -= evt.TotalValue;
                         LastEntry = $"{F.Ts(evt.OccurredAt)} -{F.Usd(evt.TotalValue)} CANCELLED";
                         break;
               }
          }
     }

     // --------------------------------------------------------------------------
     // OBSERVER 3  Alert Observer
     // Acorda atentie unui factor de sistem detectand miscari pe tipare prestabilite de frauda/risc.
     // Generati la evenimente, va returna alerta de `High value` din start, cand valoarea sare pragul predefinit.
     // --------------------------------------------------------------------------
     public class AlertObserver : IBookingObserver
     {
          public string Name => "Alert Monitor";
          public string Description => "Flags high-value bookings, cancellation bursts, and double-reservation attempts.";
          public string ColorHex => "#DC2626";

          private const decimal HighValueThreshold = 1_500m;
          private int _cancellationCount = 0;
          private readonly HashSet<string> _reservedRooms = new();

          private readonly List<AlertEntry> _alerts = new();
          public IReadOnlyList<AlertEntry> Alerts => _alerts;
          public int AlertCount => _alerts.Count;
          public string LastAlert { get; private set; } = "No alerts.";

          public void OnBookingEvent(BookingEvent evt)
          {
               switch (evt.EventType)
               {
                    case BookingEventType.BookingCreated:
                         // High-value booking alert
                         if (evt.TotalValue >= HighValueThreshold)
                              AddAlert(AlertSeverity.Warning,
                                  $"High-value booking: {F.Usd(evt.TotalValue)} ({evt.BookingType}) — Guest: {evt.GuestName}",
                                  evt.OccurredAt);

                         // Double-reservation detection
                         if (!_reservedRooms.Add(evt.RoomId))
                              AddAlert(AlertSeverity.Critical,
                                  $"Room {evt.RoomNumber} already reserved — possible double-booking!",
                                  evt.OccurredAt);
                         break;

                    case BookingEventType.BookingCancelled:
                         _cancellationCount++;
                         if (_reservedRooms.Contains(evt.RoomId))
                              _reservedRooms.Remove(evt.RoomId);

                         if (_cancellationCount >= 3)
                              AddAlert(AlertSeverity.Warning,
                                  $"Cancellation burst: {_cancellationCount} cancellations this session",
                                  evt.OccurredAt);
                         break;

                    case BookingEventType.GuestCheckedOut:
                         _reservedRooms.Remove(evt.RoomId);
                         break;
               }
          }

          private void AddAlert(AlertSeverity severity, string message, DateTime at)
          {
               _alerts.Add(new AlertEntry(severity, message, at));
               LastAlert = $"[{severity}] {message}";
          }
     }

     public enum AlertSeverity { Info, Warning, Critical }

     public sealed record AlertEntry(AlertSeverity Severity, string Message, DateTime OccurredAt)
     {
          public string ColorHex => Severity switch
          {
               AlertSeverity.Critical => "#DC2626",
               AlertSeverity.Warning => "#D97706",
               _ => "#2563EB"
          };
          public string Icon => Severity switch
          {
               AlertSeverity.Critical => "??",
               AlertSeverity.Warning => "??",
               _ => "??"
          };
     }

     // --------------------------------------------------------------------------
     // OBSERVER 4  Audit Log Observer
     // Singura lui miscare este de a lasa scris o lista audit formatata cu ora si momentul actiunii.
     // Orice eveniment este transcris prin generatoarele private.
     // --------------------------------------------------------------------------
     public class AuditLogObserver : IBookingObserver
     {
          public string Name => "Audit Log";
          public string Description => "Writes a timestamped, structured audit entry for every booking event.";
          public string ColorHex => "#7C3AED";

          private readonly List<AuditEntry> _entries = new();
          public IReadOnlyList<AuditEntry> Entries => _entries;

          public void OnBookingEvent(BookingEvent evt)
          {
               _entries.Add(new AuditEntry(
                   OccurredAt: evt.OccurredAt,
                   EventType: evt.EventType.ToString(),
                   BookingId: evt.BookingId[..8] + "…",
                   BookingType: evt.BookingType,
                   GuestName: evt.GuestName,
                   RoomNumber: evt.RoomNumber,
                   TotalValue: evt.TotalValue,
                   Details: BuildDetails(evt)
               ));
          }

          private static string BuildDetails(BookingEvent e) =>
              e.EventType switch
              {
                   BookingEventType.BookingCreated =>
                     $"Room {e.RoomNumber} · {e.Nights} night(s) · {F.Usd(e.TotalValue)} · Check-in: {e.CheckIn:dd MMM yyyy}",
                   BookingEventType.BookingConfirmed =>
                     $"Confirmed — {F.Usd(e.TotalValue)} secured",
                   BookingEventType.BookingCancelled =>
                     $"Cancelled — {F.Usd(e.TotalValue)} revenue lost",
                   BookingEventType.GuestCheckedIn =>
                     $"Checked in to Room {e.RoomNumber} — {e.Nights} nights",
                   BookingEventType.GuestCheckedOut =>
                     $"Checked out from Room {e.RoomNumber}",
                   _ => ""
              };
     }

     public sealed record AuditEntry(
         DateTime OccurredAt,
         string EventType,
         string BookingId,
         string BookingType,
         string GuestName,
         string RoomNumber,
         decimal TotalValue,
         string Details
     )
     {
          public string TimestampFmt => OccurredAt.ToString("HH:mm:ss", CultureInfo.GetCultureInfo("en-US"));
          public string TotalFmt => TotalValue.ToString("C", CultureInfo.GetCultureInfo("en-US"));

          public string BadgeColor => EventType switch
          {
               "BookingCreated" => "#2563EB",
               "BookingConfirmed" => "#15803D",
               "BookingCancelled" => "#DC2626",
               "GuestCheckedIn" => "#0891B2",
               "GuestCheckedOut" => "#7C3AED",
               _ => "#64748B"
          };
          public string BadgeIcon => EventType switch
          {
               "BookingCreated" => "+",
               "BookingConfirmed" => "?",
               "BookingCancelled" => "?",
               "GuestCheckedIn" => "?",
               "GuestCheckedOut" => "?",
               _ => "•"
          };
     }

     // --------------------------------------------------------------------------
     // OBSERVER 5  Dashboard Observer
     // Centralizator curat al sumelor (Total create, Total anulate, etc) special modelat pe GUI din ecranul Start.
     // Extrage ultimul log mereu. Face exact ce trebuie si cand trebuie, instantaneu.
     // --------------------------------------------------------------------------
     public class DashboardObserver : IBookingObserver
     {
          public string Name => "Live Dashboard";
          public string Description => "Aggregates KPI counters: total bookings, confirmed, cancelled, average stay.";
          public string ColorHex => "#F59E0B";

          private int _totalCreated = 0;
          private int _totalConfirmed = 0;
          private int _totalCancelled = 0;
          private int _totalCheckins = 0;
          private int _totalCheckouts = 0;
          private int _totalNights = 0;
          private decimal _totalRevenue = 0m;

          public int TotalCreated => _totalCreated;
          public int TotalConfirmed => _totalConfirmed;
          public int TotalCancelled => _totalCancelled;
          public int TotalCheckins => _totalCheckins;
          public int TotalCheckouts => _totalCheckouts;
          public decimal TotalRevenue => _totalRevenue;

          public decimal AvgStayNights =>
              _totalCreated == 0 ? 0 : (decimal)_totalNights / _totalCreated;

          public decimal ConfirmationRate =>
              _totalCreated == 0 ? 0 : (decimal)_totalConfirmed / _totalCreated * 100;

          public string LastActivityTime { get; private set; } = "—";
          public string LastActivityDesc { get; private set; } = "No activity yet";

          public void OnBookingEvent(BookingEvent evt)
          {
               LastActivityTime = evt.OccurredAt.ToString("HH:mm:ss");

               switch (evt.EventType)
               {
                    case BookingEventType.BookingCreated:
                         _totalCreated++;
                         _totalNights += evt.Nights;
                         _totalRevenue += evt.TotalValue;
                         LastActivityDesc = $"New booking — {evt.GuestName} · Room {evt.RoomNumber}";
                         break;
                    case BookingEventType.BookingConfirmed:
                         _totalConfirmed++;
                         LastActivityDesc = $"Confirmed — {evt.GuestName} · {evt.Nights} nights";
                         break;
                    case BookingEventType.BookingCancelled:
                         _totalCancelled++;
                         _totalRevenue -= evt.TotalValue;
                         LastActivityDesc = $"Cancelled — Room {evt.RoomNumber}";
                         break;
                    case BookingEventType.GuestCheckedIn:
                         _totalCheckins++;
                         LastActivityDesc = $"Check-in — {evt.GuestName} ? Room {evt.RoomNumber}";
                         break;
                    case BookingEventType.GuestCheckedOut:
                         _totalCheckouts++;
                         LastActivityDesc = $"Check-out — Room {evt.RoomNumber}";
                         break;
               }
          }
     }
}

