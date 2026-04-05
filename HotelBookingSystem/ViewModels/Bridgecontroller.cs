using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HotelBookingSystem.Bridge;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     public class BridgeController : BaseViewModel
     {
          private readonly IBookingRepository _bookingRepository;

          private string _selectedFormat = "Text";
          private string _selectedDelivery = "Log";
          private string _result = "Choose a format + delivery, then click Generate Report.";

          public ObservableCollection<string> Formats { get; } = new() { "Text", "HTML", "CSV" };
          public ObservableCollection<string> Deliveries { get; } = new() { "Log", "File", "Email" };

          public string SelectedFormat
          {
               get => _selectedFormat;
               set => SetProperty(ref _selectedFormat, value);
          }

          public string SelectedDelivery
          {
               get => _selectedDelivery;
               set => SetProperty(ref _selectedDelivery, value);
          }

          public string Result
          {
               get => _result;
               set => SetProperty(ref _result, value);
          }

          public event Action<string>? OnLog;

          public BridgeController(IBookingRepository bookingRepository)
          {
               _bookingRepository = bookingRepository;
          }

          /// <summary>
          /// Instantiates the correct format × delivery combination at runtime.
          /// No switch on format inside delivery or vice versa — true Bridge separation.
          /// 3 formats × 3 deliveries = 9 combinations with 6 classes total.
          /// </summary>
          public void GenerateReport()
          {
               var bookings = _bookingRepository.GetAllBookings();
               if (bookings.Count == 0)
               {
                    Result = "✗ No bookings found. Create a booking first.";
                    return;
               }

               var dispatchLog = new List<string>();
               var now = DateTime.Now;
               var periodStart = now.AddDays(-30);

               // ── IMPLEMENTATION (delivery) ──────────────────────────────────────
               IReportDelivery delivery = SelectedDelivery switch
               {
                    "File" => new FileDelivery("/reports/hotel", dispatchLog),
                    "Email" => new EmailDelivery("manager@hotel.md", dispatchLog),
                    _ => new LogDelivery(dispatchLog)
               };

               // ── ABSTRACTION (format) — injected with the delivery via Bridge ───
               HotelReport report = SelectedFormat switch
               {
                    "HTML" => new HtmlHotelReport(delivery),
                    "CSV" => new CsvHotelReport(delivery),
                    _ => new TextHotelReport(delivery)
               };

               report.Generate(bookings, periodStart, now);

               Result = $"✓ {SelectedFormat} report → {SelectedDelivery}\n" +
                        $"  Bookings included: {bookings.Count}\n" +
                        $"  Format class:   {SelectedFormat}HotelReport\n" +
                        $"  Delivery class: {SelectedDelivery}Delivery\n\n" +
                        string.Join("\n", dispatchLog);

               OnLog?.Invoke($"[Bridge] {SelectedFormat}HotelReport → {SelectedDelivery}Delivery");
               OnLog?.Invoke($"  Bookings: {bookings.Count}, Period: {periodStart:dd MMM} — {now:dd MMM}");
               foreach (var line in dispatchLog)
                    OnLog?.Invoke($"  {line}");
               OnLog?.Invoke("");
          }
     }
}