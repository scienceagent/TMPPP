using System;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.ViewModels
{
     public class BookingDetailsViewModel : BaseViewModel
     {
          public Booking Booking { get; }
          public User Guest { get; }
          public Room Room { get; }

          public string BookingId => Booking.BookingId;
          public string Status => Booking.Status.ToString();
          public string StayDates => $"{Booking.CheckInDate:dd MMM yyyy} to {Booking.CheckOutDate:dd MMM yyyy}";
          public int TotalNights => (Booking.CheckOutDate - Booking.CheckInDate).Days;

          public string GuestName => Guest.Name;
          public string GuestEmail => Guest.Email;
          public string GuestPhone => Guest.Phone;
          public string GuestDetails => Guest is Guest g ? $"Nationality: {g.Nationality} | Passport: {g.PassportNumber}" : "Staff Account";

          public string RoomNumber => Room.RoomNumber;
          public string RoomType => Room.GetType().Name.Replace("Room", "");
          public string RoomPrice => Room.BasePrice.ToString("C");
          public string RoomInfo => Room.GetDisplayInfo();

          public BookingDetailsViewModel(Booking booking, User guest, Room room)
          {
               Booking = booking;
               Guest = guest;
               Room = room;
          }
     }
}
