using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models.User;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Visitor
{
    public class StatisticsVisitor : IVisitor
    {
        public decimal TotalRevenue { get; private set; }
        public int TotalBookings { get; private set; }
        public int TotalNights { get; private set; }
        public int ActiveGuests { get; private set; }
        
        private readonly Dictionary<string, int> _roomTypeUsage = new();
        private readonly IRoomRepository _roomRepo;

        public StatisticsVisitor(IRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        public void Visit(Booking booking)
        {
            TotalBookings++;
            var nights = (booking.CheckOutDate - booking.CheckInDate).TotalDays;
            TotalNights += (int)nights;

            var room = _roomRepo.FindById(booking.RoomId);
            if (room != null)
            {
                TotalRevenue += (decimal)nights * room.BasePrice;
                
                string type = room.GetType().Name;
                if (!_roomTypeUsage.ContainsKey(type)) _roomTypeUsage[type] = 0;
                _roomTypeUsage[type]++;
            }
        }

        public void Visit(Room room)
        {
            // Could calculate maintenance costs or occupancy rates here
        }

        public void Visit(User user)
        {
            ActiveGuests++;
        }

        public string GetSummary()
        {
            var popularType = _roomTypeUsage.OrderByDescending(x => x.Value).FirstOrDefault();
            return $"--- ANALYTICS SUMMARY ---\n" +
                   $"Total Revenue: {TotalRevenue:C}\n" +
                   $"Bookings Processed: {TotalBookings}\n" +
                   $"Total Room Nights: {TotalNights}\n" +
                   $"Active Guests in System: {ActiveGuests}\n" +
                   $"Most Popular Room Type: {popularType.Key ?? "N/A"} ({popularType.Value} bookings)\n" +
                   $"Average Revenue per Booking: {(TotalBookings > 0 ? TotalRevenue / TotalBookings : 0):C}";
        }
    }
}
