using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Bridge
{
     // ── ABSTRACTION (Bridge abstraction side) ─────────────────────────────────
     // Defines WHAT the report contains (the "format" dimension).
     // Holds a reference to IReportDelivery (the bridge).
     // Subclasses override FormatContent() to produce Text / HTML / CSV output.
     public abstract class HotelReport
     {
          // ── BRIDGE — reference to the implementation hierarchy ─────────────────
          protected readonly IReportDelivery _delivery;

          protected HotelReport(IReportDelivery delivery) => _delivery = delivery;

          // Template method — async so EmailDelivery can await SMTP
          public async Task GenerateAsync(IReadOnlyList<Booking> bookings,
                                           DateTime periodStart, DateTime periodEnd)
          {
               string title = GetTitle(periodStart, periodEnd);
               string content = FormatContent(bookings, periodStart, periodEnd);
               string filename = GetFilename(periodStart);

               await _delivery.DeliverAsync(content, title, filename); // BRIDGE call
          }

          protected abstract string GetTitle(DateTime from, DateTime to);
          protected abstract string GetFilename(DateTime from);
          protected abstract string FormatContent(IReadOnlyList<Booking> bookings,
                                                   DateTime from, DateTime to);
     }

     // ── REFINED ABSTRACTION 1: Plain-text report ──────────────────────────────
     public class TextHotelReport : HotelReport
     {
          public TextHotelReport(IReportDelivery delivery) : base(delivery) { }

          protected override string GetTitle(DateTime from, DateTime to)
              => $"Hotel Booking Report (TEXT) — {from:dd MMM yyyy} to {to:dd MMM yyyy}";

          protected override string GetFilename(DateTime from)
              => $"report_text_{from:yyyyMMdd_HHmmss}.txt";

          protected override string FormatContent(IReadOnlyList<Booking> bookings,
                                                   DateTime from, DateTime to)
          {
               var sb = new StringBuilder();
               sb.AppendLine("╔══════════════════════════════════════════════════╗");
               sb.AppendLine("║        GRAND HORIZON HOTEL — BOOKING REPORT     ║");
               sb.AppendLine("╚══════════════════════════════════════════════════╝");
               sb.AppendLine($"Generated : {DateTime.Now:dd MMM yyyy HH:mm:ss}");
               sb.AppendLine($"Period    : {from:dd MMM yyyy} — {to:dd MMM yyyy}");
               sb.AppendLine($"Total     : {bookings.Count} bookings");
               sb.AppendLine(new string('─', 72));
               sb.AppendLine($"{"ID",-12} {"Type",-10} {"Status",-12} {"Check-In",-14} {"Check-Out",-14} {"Nights",6}");
               sb.AppendLine(new string('─', 72));

               foreach (var b in bookings)
               {
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;
                    sb.AppendLine(
                        $"{b.BookingId[..8],-12} " +
                        $"{b.BookingType,-10} " +
                        $"{b.Status,-12} " +
                        $"{b.CheckInDate:dd MMM yyyy,-14} " +
                        $"{b.CheckOutDate:dd MMM yyyy,-14} " +
                        $"{nights,6}");
               }

               sb.AppendLine(new string('═', 72));
               sb.AppendLine($"Confirmed : {bookings.Count(b => b.Status == BookingStatus.Confirmed)}");
               sb.AppendLine($"Pending   : {bookings.Count(b => b.Status == BookingStatus.Pending)}");
               sb.AppendLine($"Cancelled : {bookings.Count(b => b.Status == BookingStatus.Cancelled)}");
               return sb.ToString();
          }
     }

     // ── REFINED ABSTRACTION 2: HTML report ───────────────────────────────────
     public class HtmlHotelReport : HotelReport
     {
          public HtmlHotelReport(IReportDelivery delivery) : base(delivery) { }

          protected override string GetTitle(DateTime from, DateTime to)
              => $"Hotel Booking Report (HTML) — {from:dd MMM yyyy} to {to:dd MMM yyyy}";

          protected override string GetFilename(DateTime from)
              => $"report_html_{from:yyyyMMdd_HHmmss}.html";

          protected override string FormatContent(IReadOnlyList<Booking> bookings,
                                                   DateTime from, DateTime to)
          {
               var rows = new StringBuilder();
               foreach (var b in bookings)
               {
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;
                    string color = b.Status switch
                    {
                         BookingStatus.Confirmed => "#D4EDDA",
                         BookingStatus.Cancelled => "#F8D7DA",
                         _ => "#FFF3CD"
                    };
                    rows.AppendLine(
                        $"<tr style='background:{color}'>" +
                        $"<td>{b.BookingId[..8]}…</td>" +
                        $"<td>{b.BookingType}</td>" +
                        $"<td><strong>{b.Status}</strong></td>" +
                        $"<td>{b.CheckInDate:dd MMM yyyy}</td>" +
                        $"<td>{b.CheckOutDate:dd MMM yyyy}</td>" +
                        $"<td style='text-align:center'>{nights}</td>" +
                        $"</tr>");
               }

               return $@"<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'/>
  <title>Hotel Report</title>
  <style>
    body   {{ font-family: Segoe UI, Arial, sans-serif; margin: 32px; color: #212529; }}
    h1     {{ color: #0D1B2A; border-bottom: 2px solid #2E9CCA; padding-bottom: 8px; }}
    .meta  {{ color: #6C757D; font-size: 13px; margin-bottom: 20px; }}
    table  {{ border-collapse: collapse; width: 100%; font-size: 13px; }}
    th     {{ background: #0D1B2A; color: white; padding: 10px 14px; text-align: left; }}
    td     {{ padding: 8px 14px; border-bottom: 1px solid #DEE2E6; }}
    .total {{ margin-top: 16px; font-weight: bold; color: #0D1B2A; }}
  </style>
</head>
<body>
  <h1>🏨 Grand Horizon Hotel — Booking Report</h1>
  <p class='meta'>Generated: {DateTime.Now:dd MMM yyyy HH:mm:ss} &nbsp;|&nbsp;
                  Period: {from:dd MMM yyyy} – {to:dd MMM yyyy} &nbsp;|&nbsp;
                  Total: {bookings.Count} bookings</p>
  <table>
    <thead>
      <tr>
        <th>Booking ID</th><th>Type</th><th>Status</th>
        <th>Check-In</th><th>Check-Out</th><th>Nights</th>
      </tr>
    </thead>
    <tbody>
      {rows}
    </tbody>
  </table>
  <p class='total'>
    ✅ Confirmed: {bookings.Count(b => b.Status == BookingStatus.Confirmed)} &nbsp;|&nbsp;
    ⏳ Pending: {bookings.Count(b => b.Status == BookingStatus.Pending)} &nbsp;|&nbsp;
    ❌ Cancelled: {bookings.Count(b => b.Status == BookingStatus.Cancelled)}
  </p>
</body>
</html>";
          }
     }

     // ── REFINED ABSTRACTION 3: CSV report ─────────────────────────────────────
     public class CsvHotelReport : HotelReport
     {
          public CsvHotelReport(IReportDelivery delivery) : base(delivery) { }

          protected override string GetTitle(DateTime from, DateTime to)
              => $"Hotel Booking Report (CSV) — {from:dd MMM yyyy} to {to:dd MMM yyyy}";

          protected override string GetFilename(DateTime from)
              => $"report_csv_{from:yyyyMMdd_HHmmss}.csv";

          protected override string FormatContent(IReadOnlyList<Booking> bookings,
                                                   DateTime from, DateTime to)
          {
               var sb = new StringBuilder();
               sb.AppendLine("BookingId,Type,Status,CheckIn,CheckOut,Nights");
               foreach (var b in bookings)
               {
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;
                    sb.AppendLine(
                        $"{b.BookingId[..8]}...," +
                        $"{b.BookingType}," +
                        $"{b.Status}," +
                        $"{b.CheckInDate:yyyy-MM-dd}," +
                        $"{b.CheckOutDate:yyyy-MM-dd}," +
                        $"{nights}");
               }
               return sb.ToString();
          }
     }
}