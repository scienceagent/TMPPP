using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.State
{
    // ─── PENDING ───────────────────────────────────────────────────────────
    public class PendingState : IBookingState
    {
        public string StateName => "Pending";
        public string Description => "The booking has been submitted and is awaiting confirmation.";
        public string StatusColor => "#94A3B8";

        public void Confirm(Booking booking) => booking.SetState(new ConfirmedState());
        public void CheckIn(Booking booking) => throw new InvalidOperationException("Confirm the booking before check-in.");
        public void Complete(Booking booking) => throw new InvalidOperationException("Cannot complete a pending booking.");
        public void Cancel(Booking booking) => booking.SetState(new CancelledState());
    }

    // ─── CONFIRMED ─────────────────────────────────────────────────────────
    public class ConfirmedState : IBookingState
    {
        public string StateName => "Confirmed";
        public string Description => "The booking is confirmed. Waiting for the guest to arrive.";
        public string StatusColor => "#3B82F6";

        public void Confirm(Booking booking) => Console.WriteLine("Already confirmed.");
        public void CheckIn(Booking booking) => booking.SetState(new CheckedInState());
        public void Complete(Booking booking) => throw new InvalidOperationException("Guest must check-in before completion.");
        public void Cancel(Booking booking) => booking.SetState(new CancelledState());
    }

    // ─── CHECKED-IN ────────────────────────────────────────────────────────
    public class CheckedInState : IBookingState
    {
        public string StateName => "Checked-In";
        public string Description => "The guest is currently in-house.";
        public string StatusColor => "#10B981";

        public void Confirm(Booking booking) => throw new InvalidOperationException("Already in-house.");
        public void CheckIn(Booking booking) => Console.WriteLine("Already checked in.");
        public void Complete(Booking booking) => booking.SetState(new CompletedState());
        public void Cancel(Booking booking) => throw new InvalidOperationException("Cannot cancel after check-in.");
    }

    // ─── COMPLETED ─────────────────────────────────────────────────────────
    public class CompletedState : IBookingState
    {
        public string StateName => "Completed";
        public string Description => "The stay is finished. Booking record is closed.";
        public string StatusColor => "#6366F1";

        public void Confirm(Booking booking) => throw new InvalidOperationException("Already completed.");
        public void CheckIn(Booking booking) => throw new InvalidOperationException("Already completed.");
        public void Complete(Booking booking) => Console.WriteLine("Already completed.");
        public void Cancel(Booking booking) => throw new InvalidOperationException("Already completed.");
    }

    // ─── CANCELLED ─────────────────────────────────────────────────────────
    public class CancelledState : IBookingState
    {
        public string StateName => "Cancelled";
        public string Description => "The booking was cancelled.";
        public string StatusColor => "#EF4444";

        public void Confirm(Booking booking) => throw new InvalidOperationException("Already cancelled.");
        public void CheckIn(Booking booking) => throw new InvalidOperationException("Already cancelled.");
        public void Complete(Booking booking) => throw new InvalidOperationException("Already cancelled.");
        public void Cancel(Booking booking) => Console.WriteLine("Already cancelled.");
    }

    // ─── STATE FACTORY ─────────────────────────────────────────────────────
    public static class BookingStateFactory
    {
        public static IBookingState GetState(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => new PendingState(),
                BookingStatus.Confirmed => new ConfirmedState(),
                BookingStatus.CheckedIn => new CheckedInState(),
                BookingStatus.Completed => new CompletedState(),
                BookingStatus.Cancelled => new CancelledState(),
                _ => new PendingState()
            };
        }

        public static BookingStatus GetStatus(IBookingState state)
        {
            return state switch
            {
                PendingState => BookingStatus.Pending,
                ConfirmedState => BookingStatus.Confirmed,
                CheckedInState => BookingStatus.CheckedIn,
                CompletedState => BookingStatus.Completed,
                CancelledState => BookingStatus.Cancelled,
                _ => BookingStatus.Pending
            };
        }
    }
}
