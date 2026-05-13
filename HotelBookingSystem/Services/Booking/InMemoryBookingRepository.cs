using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Data;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Services
{
     public class InMemoryBookingRepository : IBookingRepository
     {
          public Booking FindById(string id)
          {
              using var context = new AppDbContext();
              return context.Bookings.FirstOrDefault(b => b.BookingId == id);
          }

          public List<Booking> GetUserBookings(string userId)
          {
              using var context = new AppDbContext();
              return context.Bookings.Where(b => b.UserId == userId).ToList();
          }

          public List<Booking> GetAllBookings()
          {
              using var context = new AppDbContext();
              return context.Bookings.ToList();
          }

          public void Save(Booking booking)
          {
              using var context = new AppDbContext();
              var existing = context.Bookings.FirstOrDefault(b => b.BookingId == booking.BookingId);
              if (existing != null)
              {
                  context.Entry(existing).CurrentValues.SetValues(booking);
              }
              else
              {
                  context.Bookings.Add(booking);
              }
              context.SaveChanges();
          }
     }
}