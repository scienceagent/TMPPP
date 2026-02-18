using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class InMemoryBookingRepository : IBookingRepository
     {
          private readonly List<Booking> _bookings = new List<Booking>();

          public Booking FindById(string id) =>
              _bookings.FirstOrDefault(b => b.BookingId == id);

          public List<Booking> GetUserBookings(string userId) =>
              _bookings.Where(b => b.UserId == userId).ToList();

          public List<Booking> GetAllBookings() => new List<Booking>(_bookings);

          public void Save(Booking booking)
          {
               var existing = FindById(booking.BookingId);
               if (existing != null) _bookings.Remove(existing);
               _bookings.Add(booking);
          }
     }
}