using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ChainOfResponsibility
{
    /// <summary>
    /// Step 1: Validates that the guest exists in the database.
    /// </summary>
    public class GuestValidationHandler : BaseBookingHandler
    {
        private readonly IUserRepository _userRepository;

        public GuestValidationHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override BookingProcessResult Handle(BookingProcessRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GuestId))
            {
                return new BookingProcessResult(false, "Guest Validation", 
                    "Invalid Guest ID. A registered guest is required to proceed.", "#EF4444");
            }

            var user = _userRepository.FindById(request.GuestId);
            if (user == null)
            {
                return new BookingProcessResult(false, "Guest Validation", 
                    $"User with ID {request.GuestId} not found in database.", "#EF4444");
            }

            return PassToNext(request);
        }
    }

    /// <summary>
    /// Step 2: Checks if the requested room exists and is available in the database.
    /// </summary>
    public class RoomAvailabilityHandler : BaseBookingHandler
    {
        private readonly IRoomRepository _roomRepository;

        public RoomAvailabilityHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public override BookingProcessResult Handle(BookingProcessRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoomId))
            {
                return new BookingProcessResult(false, "Room Availability", 
                    "No room selected. Please specify a room ID.", "#EF4444");
            }

            var room = _roomRepository.FindById(request.RoomId);
            if (room == null)
            {
                return new BookingProcessResult(false, "Room Availability", 
                    $"Room with ID {request.RoomId} not found.", "#EF4444");
            }

            if (!room.IsAvailable)
            {
                return new BookingProcessResult(false, "Room Availability", 
                    $"Room {room.RoomNumber} is currently occupied or under maintenance.", "#EF4444");
            }

            if (request.CheckIn >= request.CheckOut)
            {
                return new BookingProcessResult(false, "Date Validation", 
                    "Check-out date must be after check-in date.", "#EF4444");
            }

            return PassToNext(request);
        }
    }

    /// <summary>
    /// Step 3: Verifies that the stay duration respects the hotel's policies.
    /// </summary>
    public class StayDurationHandler : BaseBookingHandler
    {
        public override BookingProcessResult Handle(BookingProcessRequest request)
        {
            var duration = (request.CheckOut - request.CheckIn).TotalDays;

            if (duration < 1)
            {
                return new BookingProcessResult(false, "Policy Check", 
                    "Minimum stay is 1 night.", "#F59E0B");
            }

            bool isWeekend = request.CheckIn.DayOfWeek == DayOfWeek.Friday || request.CheckIn.DayOfWeek == DayOfWeek.Saturday;
            if (isWeekend && duration < 2)
            {
                return new BookingProcessResult(false, "Policy Check", 
                    "Weekend bookings require a minimum stay of 2 nights.", "#F59E0B");
            }

            return PassToNext(request);
        }
    }

    /// <summary>
    /// Step 4: Final verification of the total price against the database base price.
    /// </summary>
    public class PriceVerificationHandler : BaseBookingHandler
    {
        private readonly IRoomRepository _roomRepository;

        public PriceVerificationHandler(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public override BookingProcessResult Handle(BookingProcessRequest request)
        {
            var room = _roomRepository.FindById(request.RoomId);
            if (room == null) return PassToNext(request); // Should have been caught by Step 2

            if (request.ExpectedPrice < room.BasePrice)
            {
                return new BookingProcessResult(false, "Financial Review", 
                    $"Requested price (${request.ExpectedPrice}) is below the room base price (${room.BasePrice}).", "#EF4444");
            }

            if (request.ExpectedPrice > 10000)
            {
                return new BookingProcessResult(true, "Senior Management", 
                    "High-value booking detected. Requires manual oversight but marked as pre-approved.", "#3B82F6");
            }

            return PassToNext(request);
        }
    }
}
