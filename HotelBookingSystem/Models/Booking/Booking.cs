using System;
using HotelBookingSystem.State;

namespace HotelBookingSystem.Models
{
     public class Booking : HotelBookingSystem.Visitor.IVisitable
     {
          public void Accept(HotelBookingSystem.Visitor.IVisitor visitor) => visitor.Visit(this);

          public string BookingId { get; set; }
          public string UserId { get; set; }
          public string RoomId { get; set; }
          public DateTime CheckInDate { get; set; }
          public DateTime CheckOutDate { get; set; }
          public string BookingType { get; set; }
          public BookingStatus Status { get; set; }

          // Pattern State: internal behavior delegate
          private IBookingState _state;
          public IBookingState CurrentState => _state ??= BookingStateFactory.GetState(Status);

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
               _state = new PendingState();
          }

          // Method for the State pattern to trigger transitions
          public void SetState(IBookingState newState)
          {
               _state = newState;
               Status = BookingStateFactory.GetStatus(newState);
          }

          public void Confirm() => CurrentState.Confirm(this);
          public void CheckIn() => CurrentState.CheckIn(this);
          public void Cancel() => CurrentState.Cancel(this);
          public void Complete() => CurrentState.Complete(this);
     }
}