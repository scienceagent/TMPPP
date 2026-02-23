using System.Collections.Generic;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Interfaces
{
     public interface IBookingRepository
     {
          Booking FindById(string id);
          List<Booking> GetUserBookings(string userId);
          List<Booking> GetAllBookings();
          void Save(Booking booking);
     }
}