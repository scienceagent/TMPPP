using System.Collections.Generic;
using HotelBookingSystem.Adapter;
using HotelBookingSystem.Composite;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Facade
{
     public class HotelFacade
     {
          private readonly IBookingService _bookingService;
          private readonly IBookingRepository _bookingRepository;
          private readonly IRoomRepository _roomRepository;
          private readonly IUserRepository _userRepository;
          private readonly IPaymentService _paymentService;
          private readonly ILogger _logger;

          public HotelFacade(IBookingService bookingService, IBookingRepository bookingRepository,
              IRoomRepository roomRepository, IUserRepository userRepository,
              IPaymentService paymentService, ILogger logger)
          {
               _bookingService = bookingService;
               _bookingRepository = bookingRepository;
               _roomRepository = roomRepository;
               _userRepository = userRepository;
               _paymentService = paymentService;
               _logger = logger;
          }

          public CheckInResult CheckInGuest(string bookingId)
          {
               _logger.Info($"[Facade] CheckIn: {bookingId}");
               var booking = _bookingRepository.FindById(bookingId);
               if (booking == null) return CheckInResult.Fail("Booking not found.");
               if (booking.Status != BookingStatus.Confirmed)
                    return CheckInResult.Fail("Booking must be Confirmed before check-in.");

               var room = _roomRepository.FindById(booking.RoomId);
               var user = _userRepository.FindById(booking.UserId);
               if (room == null || user == null) return CheckInResult.Fail("Room or guest not found.");

               decimal total = room.BasePrice * (booking.CheckOutDate - booking.CheckInDate).Days;
               bool paid = _paymentService.ProcessPayment(user.Id, total);
               if (!paid) return CheckInResult.Fail("Payment failed.");

               string txId = _paymentService.GetLastTransactionId();
               _logger.Info($"[Facade] Paid ${total:F2} — TX: {txId}");
               return CheckInResult.Ok(user.Name, room.RoomNumber, total, txId);
          }

          public CheckOutResult CheckOutGuest(string bookingId, IReadOnlyList<RoomServiceComponent> services)
          {
               _logger.Info($"[Facade] CheckOut: {bookingId}");
               var booking = _bookingRepository.FindById(bookingId);
               if (booking == null) return CheckOutResult.Fail("Booking not found.");

               var room = _roomRepository.FindById(booking.RoomId);
               var user = _userRepository.FindById(booking.UserId);

               decimal servicesTotal = 0;
               var lines = new List<string>();
               foreach (var svc in services)
               {
                    servicesTotal += svc.GetPrice();
                    lines.Add($"  • {svc.Name}: ${svc.GetPrice():F2}");
               }
               if (servicesTotal > 0)
                    _paymentService.ProcessPayment(user?.Id ?? "", servicesTotal);

               _bookingService.CancelBooking(bookingId);
               _logger.Info($"[Facade] Room {room?.RoomNumber} released. Services: ${servicesTotal:F2}");
               return CheckOutResult.Ok(user?.Name ?? "Guest", room?.RoomNumber ?? "-", servicesTotal, lines);
          }

          public string GetBookingSummary(string bookingId)
          {
               var booking = _bookingRepository.FindById(bookingId);
               if (booking == null) return "Booking not found.";
               var room = _roomRepository.FindById(booking.RoomId);
               var user = _userRepository.FindById(booking.UserId);
               int nights = (booking.CheckOutDate - booking.CheckInDate).Days;
               decimal total = (room?.BasePrice ?? 0) * nights;
               return $"Booking {booking.BookingId[..8]}...\n" +
                      $"Guest: {user?.Name ?? "Unknown"}\n" +
                      $"Room: {room?.RoomNumber ?? "-"} | Type: {booking.BookingType}\n" +
                      $"Dates: {booking.CheckInDate:dd MMM} → {booking.CheckOutDate:dd MMM} ({nights} nights)\n" +
                      $"Estimated total: ${total:F2}\nStatus: {booking.Status}";
          }
     }
}