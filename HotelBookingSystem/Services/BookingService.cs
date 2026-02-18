using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class BookingService : IBookingService
     {
          private readonly IBookingRepository _bookingRepository;
          private readonly IRoomRepository _roomRepository;
          private readonly IUserRepository _userRepository;
          private readonly IBookingConfirmationService _confirmationService;
          private readonly IUserValidator _userValidator;
          private readonly ILogger _logger;

          public BookingService(
              IBookingRepository bookingRepository,
              IRoomRepository roomRepository,
              IUserRepository userRepository,
              IBookingConfirmationService confirmationService,
              IUserValidator userValidator,
              ILogger logger)
          {
               _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
               _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
               _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
               _confirmationService = confirmationService ?? throw new ArgumentNullException(nameof(confirmationService));
               _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
               _logger = logger ?? throw new ArgumentNullException(nameof(logger));
          }

          public BookingResult CreateBooking(Booking booking)
          {
               if (booking == null)
               {
                    _logger.Warn("Booking is null");
                    return BookingResult.Fail("Booking is null");
               }

               _logger.Info($"Creating booking {booking.BookingId}");

               var user = _userRepository.FindById(booking.UserId);
               if (user == null)
               {
                    _logger.Warn($"User {booking.UserId} not found");
                    return BookingResult.Fail("User not found");
               }

               if (!_userValidator.Validate(user))
               {
                    _logger.Warn($"User {booking.UserId} validation failed");
                    return BookingResult.Fail("User validation failed");
               }

               var room = _roomRepository.FindById(booking.RoomId);
               if (room == null)
               {
                    _logger.Warn($"Room {booking.RoomId} not found");
                    return BookingResult.Fail("Room not found");
               }

               if (!room.IsAvailable)
               {
                    _logger.Warn($"Room {booking.RoomId} not available");
                    return BookingResult.Fail("Room not available");
               }

               _bookingRepository.Save(booking);
               _logger.Info($"Created booking {booking.BookingId} (Pending)");
               return BookingResult.Ok("Booking created");
          }

          public BookingResult ConfirmBooking(string bookingId)
          {
               _logger.Info($"Confirming booking {bookingId}");

               var booking = _bookingRepository.FindById(bookingId);
               if (booking == null) return BookingResult.Fail("Booking not found");

               var room = _roomRepository.FindById(booking.RoomId);
               if (room == null) return BookingResult.Fail("Room not found");

               try
               {
                    _confirmationService.ConfirmBooking(booking, room);
                    _bookingRepository.Save(booking);
                    _roomRepository.Save(room);
                    _logger.Info($"Confirmed booking {bookingId}");
                    return BookingResult.Ok("Booking confirmed");
               }
               catch (Exception ex)
               {
                    _logger.Error($"Confirm error for {bookingId}: {ex.Message}");
                    return BookingResult.Fail($"Confirm failed: {ex.Message}");
               }
          }

          public BookingResult CancelBooking(string bookingId)
          {
               _logger.Info($"Cancelling booking {bookingId}");

               var booking = _bookingRepository.FindById(bookingId);
               if (booking == null) return BookingResult.Fail("Booking not found");

               var room = _roomRepository.FindById(booking.RoomId);
               if (room == null) return BookingResult.Fail("Room not found");

               try
               {
                    _confirmationService.CancelBooking(booking, room);
                    _bookingRepository.Save(booking);
                    _roomRepository.Save(room);
                    _logger.Info($"Cancelled booking {bookingId}");
                    return BookingResult.Ok("Booking cancelled");
               }
               catch (Exception ex)
               {
                    _logger.Error($"Cancel error for {bookingId}: {ex.Message}");
                    return BookingResult.Fail($"Cancel failed: {ex.Message}");
               }
          }
     }
}