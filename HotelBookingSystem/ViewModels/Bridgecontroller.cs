using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using HotelBookingSystem.Bridge;
using HotelBookingSystem.Config;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.ViewModels
{
     public class BridgeController : BaseViewModel
     {
          private readonly IBookingRepository _bookingRepository;

          private string _selectedFormat = "Text";
          private string _selectedDelivery = "Log";
          private string _result = "Choose a format + delivery, then click Generate Report.";
          private bool _isBusy;

          public ObservableCollection<string> Formats { get; } = new() { "Text", "HTML", "CSV" };
          public ObservableCollection<string> Deliveries { get; } = new() { "Log", "File", "Email", "File + Email" };

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

          public bool IsBusy
          {
               get => _isBusy;
               set => SetProperty(ref _isBusy, value);
          }

          public event Action<string>? OnLog;

          public BridgeController(IBookingRepository bookingRepository)
          {
               _bookingRepository = bookingRepository;
          }

          // ── Main async entry point ─────────────────────────────────────────────
          public async Task GenerateReportAsync()
          {
               var bookings = _bookingRepository.GetAllBookings();
               if (bookings.Count == 0)
               {
                    Result = "✗ No bookings found. Create a booking first.";
                    return;
               }

               IsBusy = true;
               Result = $"⏳ Generating {SelectedFormat} report via {SelectedDelivery}…";

               var dispatchLog = new List<string>();
               var now = DateTime.Now;
               var periodStart = now.AddDays(-30);
               string managerEmail = AppSettings.Instance.GmailDefaults.Email;

               try
               {
                    // ── BRIDGE IMPLEMENTATION (delivery) ─────────────────────────
                    IReportDelivery delivery = SelectedDelivery switch
                    {
                         "File" => new FileDelivery(dispatchLog),
                         "Email" => new EmailDelivery(managerEmail, dispatchLog),
                         "File + Email" => new FileAndEmailDelivery(managerEmail, dispatchLog),
                         _ => new LogDelivery(dispatchLog)
                    };

                    // ── BRIDGE ABSTRACTION (format) — injected with delivery ──────
                    HotelReport report = SelectedFormat switch
                    {
                         "HTML" => new HtmlHotelReport(delivery),
                         "CSV" => new CsvHotelReport(delivery),
                         _ => new TextHotelReport(delivery)
                    };

                    await report.GenerateAsync(bookings, periodStart, now);

                    // Build summary
                    string outputInfo = SelectedDelivery switch
                    {
                         "File" => $"  Saved to: {AppSettings.Instance.ReportSettings.OutputDirectory}",
                         "Email" => $"  Emailed to: {managerEmail}",
                         "File + Email" => $"  Saved to: {AppSettings.Instance.ReportSettings.OutputDirectory}\n" +
                                           $"  Emailed to: {managerEmail}",
                         _ => "  Written to activity log"
                    };

                    Result =
                        $"✓ {SelectedFormat} report generated via {SelectedDelivery}\n" +
                        $"  Bookings: {bookings.Count}\n" +
                        $"  Period  : {periodStart:dd MMM yyyy} — {now:dd MMM yyyy}\n" +
                        $"{outputInfo}\n\n" +
                        string.Join("\n", dispatchLog);

                    OnLog?.Invoke($"[Bridge] {SelectedFormat}Report × {SelectedDelivery}Delivery");
                    foreach (var line in dispatchLog)
                         OnLog?.Invoke($"  {line}");
                    OnLog?.Invoke("");
               }
               catch (Exception ex)
               {
                    Result = $"✗ Report generation failed: {ex.Message}";
                    OnLog?.Invoke($"[Bridge] ERROR: {ex.Message}");
               }
               finally
               {
                    IsBusy = false;
               }
          }
     }
}