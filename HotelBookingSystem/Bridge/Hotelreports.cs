using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Bridge
{
     /// <summary>
     /// Refined Abstraction 1 — Plain-text report format.
     /// Can be combined with ANY delivery: LogDelivery, FileDelivery, EmailDelivery.
     /// </summary>
     public class TextHotelReport : HotelReport
     {
          public TextHotelReport(IReportDelivery delivery) : base(delivery) { }

          protected override string FormatContent(IReadOnlyList<Booking> bookings,
              DateTime periodStart, DateTime periodEnd)
          {
               var sb = new StringBuilder();
               sb.AppendLine("=== HOTEL BOOKING REPORT (TEXT) ===");
               sb.AppendLine($"Period : {periodStart:dd MMM yyyy} — {periodEnd:dd MMM yyyy}");
               sb.AppendLine($"Total  : {bookings.Count} bookings");
               sb.AppendLine(new string('-', 50));
               foreach (var b in bookings)
                    sb.AppendLine($"  {b.BookingId[..8]}... | {b.BookingType,-10} | {b.Status,-12} | " +
                                  $"{b.CheckInDate:dd MMM} → {b.CheckOutDate:dd MMM}");
               sb.AppendLine(new string('=', 50));
               return sb.ToString();
          }
     }

     /// <summary>
     /// Refined Abstraction 2 — HTML report format.
     /// Zero code shared with TextHotelReport; both can use any IReportDelivery.
     /// </summary>
     public class HtmlHotelReport : HotelReport
     {
          public HtmlHotelReport(IReportDelivery delivery) : base(delivery) { }

          protected override string FormatContent(IReadOnlyList<Booking> bookings,
              DateTime periodStart, DateTime periodEnd)
          {
               var rows = string.Join("\n", bookings.Select(b =>
                   $"<tr><td>{b.BookingId[..8]}...</td><td>{b.BookingType}</td>" +
                   $"<td>{b.Status}</td><td>{b.CheckInDate:dd MMM}</td><td>{b.CheckOutDate:dd MMM}</td></tr>"));

               return $@"<!DOCTYPE html>
<html><head><title>Hotel Report</title></head><body>
<h2>Hotel Booking Report</h2>
<p>Period: <b>{periodStart:dd MMM yyyy}</b> to <b>{periodEnd:dd MMM yyyy}</b> | Total: {bookings.Count}</p>
<table border='1' cellpadding='6'>
  <thead><tr><th>Booking ID</th><th>Type</th><th>Status</th><th>Check-In</th><th>Check-Out</th></tr></thead>
  <tbody>{rows}</tbody>
</table>
</body></html>";
          }
     }

     /// <summary>
     /// Refined Abstraction 3 — CSV report format.
     /// Same delivery options, completely different output.
     /// </summary>
     public class CsvHotelReport : HotelReport
     {
          public CsvHotelReport(IReportDelivery delivery) : base(delivery) { }

          protected override string FormatContent(IReadOnlyList<Booking> bookings,
              DateTime periodStart, DateTime periodEnd)
          {
               var sb = new StringBuilder();
               sb.AppendLine("BookingId,Type,Status,CheckIn,CheckOut,Nights");
               foreach (var b in bookings)
               {
                    int nights = (b.CheckOutDate - b.CheckInDate).Days;
                    sb.AppendLine($"{b.BookingId[..8]}...,{b.BookingType},{b.Status}," +
                                  $"{b.CheckInDate:yyyy-MM-dd},{b.CheckOutDate:yyyy-MM-dd},{nights}");
               }
               return sb.ToString();
          }
     }
}