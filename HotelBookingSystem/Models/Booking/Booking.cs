using System;

namespace HotelBookingSystem.Models
{
     public class Booking
     {
          public string BookingId { get; }
          public string UserId { get; }
          public string RoomId { get; }
          public DateTime CheckInDate { get; }
          public DateTime CheckOutDate { get; }
          public string BookingType { get; }
          public BookingStatus Status { get; private set; }

          public Booking(string bookingId, string userId, string roomId,
                         DateTime checkInDate, DateTime checkOutDate,
                         string bookingType = "Standard")
          {
               BookingId = bookingId;
               UserId = userId;
               RoomId = roomId;
               CheckInDate = checkInDate;
               CheckOutDate = checkOutDate;
               BookingType = bookingType;
               Status = BookingStatus.Pending;
          }

          public void Confirm()
          {
               if (Status != BookingStatus.Pending)
                    throw new InvalidOperationException("Only pending bookings can be confirmed.");
               Status = BookingStatus.Confirmed;
          }

          public void Cancel()
          {
               if (Status == BookingStatus.Cancelled)
                    throw new InvalidOperationException("Booking already cancelled.");
               Status = BookingStatus.Cancelled;
          }

          public void Complete()
          {
               if (Status != BookingStatus.Confirmed)
                    throw new InvalidOperationException("Only confirmed bookings can be completed.");
               Status = BookingStatus.Completed;
          }
     }
}