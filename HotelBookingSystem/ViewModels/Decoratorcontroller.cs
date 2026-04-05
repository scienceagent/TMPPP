using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HotelBookingSystem.Decorator;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     public class DecoratorController : BaseViewModel
     {
          private readonly IBookingRepository _bookingRepository;

          private bool _useLogging = true;
          private bool _useEmail = true;
          private bool _useSms = false;
          private string _result = "Configure decorators above, select a booking, then fire a notification.";
          private string? _selectedBookingId;

          public bool UseLogging
          {
               get => _useLogging;
               set => SetProperty(ref _useLogging, value);
          }

          public bool UseEmail
          {
               get => _useEmail;
               set => SetProperty(ref _useEmail, value);
          }

          public bool UseSms
          {
               get => _useSms;
               set => SetProperty(ref _useSms, value);
          }

          public string Result
          {
               get => _result;
               set => SetProperty(ref _result, value);
          }

          public string? SelectedBookingId
          {
               get => _selectedBookingId;
               set => SetProperty(ref _selectedBookingId, value);
          }

          public ObservableCollection<string> BookingIds { get; } = new();

          public event Action<string>? OnLog;

          public DecoratorController(IBookingRepository bookingRepository)
          {
               _bookingRepository = bookingRepository;
          }

          public void RefreshBookings()
          {
               BookingIds.Clear();
               foreach (var b in _bookingRepository.GetAllBookings())
                    BookingIds.Add(b.BookingId);

               if (BookingIds.Count > 0 && string.IsNullOrEmpty(SelectedBookingId))
                    SelectedBookingId = BookingIds[0];
          }

          /// <summary>
          /// Builds the decorator chain at runtime based on checkboxes,
          /// then fires NotifyBookingCreated.
          /// Chain order: Logging → Email → SMS → CoreService
          /// </summary>
          public void FireCreated() => Fire("Created");
          public void FireConfirmed() => Fire("Confirmed");
          public void FireCancelled() => Fire("Cancelled");

          private void Fire(string eventType)
          {
               if (string.IsNullOrEmpty(SelectedBookingId))
               {
                    Result = "✗ No booking selected.";
                    return;
               }

               var booking = _bookingRepository.FindById(SelectedBookingId);
               if (booking == null) { Result = "✗ Booking not found."; return; }

               var dispatchLog = new List<string>();

               // ── BUILD DECORATOR CHAIN ──────────────────────────────────────────
               // Start with the concrete component (always present)
               IBookingNotificationService service = new BookingNotificationService();

               // Wrap with decorators in reverse order (innermost first)
               if (UseSms) service = new SmsNotificationDecorator(service, dispatchLog);
               if (UseEmail) service = new EmailNotificationDecorator(service, dispatchLog);
               if (UseLogging) service = new LoggingNotificationDecorator(service, dispatchLog);

               // ── FIRE ──────────────────────────────────────────────────────────
               switch (eventType)
               {
                    case "Created": service.NotifyBookingCreated(booking); break;
                    case "Confirmed": service.NotifyBookingConfirmed(booking); break;
                    case "Cancelled": service.NotifyBookingCancelled(booking); break;
               }

               // ── SHOW CHAIN IN RESULT ──────────────────────────────────────────
               string chainDesc = BuildChainDescription();
               Result = $"✓ Notify{eventType} fired.\n\nChain: {chainDesc}\n\n" +
                        string.Join("\n", dispatchLog);

               OnLog?.Invoke($"[Decorator] Notify{eventType}({booking.BookingId[..8]}...)");
               foreach (var line in dispatchLog)
                    OnLog?.Invoke($"  {line}");
               OnLog?.Invoke("");
          }

          private string BuildChainDescription()
          {
               var parts = new List<string>();
               if (UseLogging) parts.Add("LoggingDecorator");
               if (UseEmail) parts.Add("EmailDecorator");
               if (UseSms) parts.Add("SmsDecorator");
               parts.Add("CoreNotificationService");
               return string.Join(" → ", parts);
          }
     }
}