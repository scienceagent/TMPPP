using System;
using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Bridge
{
     /// <summary>
     /// Bridge — ABSTRACTION hierarchy.
     /// Defines WHAT the report contains (the "format" dimension).
     /// Holds a reference to IReportDelivery (the bridge/punte).
     /// Subclasses override FormatContent() to produce Text / HTML / CSV output.
     /// Adding a new format (e.g. XML, PDF) requires only a new subclass here —
     /// zero changes to any delivery class.
     /// </summary>
     public abstract class HotelReport
     {
          // ── BRIDGE — reference to the implementation hierarchy ─────────────────
          protected readonly IReportDelivery _delivery;

          protected HotelReport(IReportDelivery delivery)
          {
               _delivery = delivery;
          }

          /// <summary>
          /// Template method — calls FormatContent() then delegates to _delivery.
          /// </summary>
          public void Generate(IReadOnlyList<Booking> bookings, DateTime periodStart, DateTime periodEnd)
          {
               string title = $"Hotel Report {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}";
               string content = FormatContent(bookings, periodStart, periodEnd);
               _delivery.Deliver(content, title);   // ← BRIDGE: delegated to implementation
          }

          protected abstract string FormatContent(
              IReadOnlyList<Booking> bookings,
              DateTime periodStart,
              DateTime periodEnd);
     }
}