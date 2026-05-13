using System;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.State
{
    public interface IBookingState
    {
        string StateName { get; }
        string Description { get; }
        string StatusColor { get; }

        void Confirm(Booking booking);
        void CheckIn(Booking booking);
        void Complete(Booking booking);
        void Cancel(Booking booking);
    }
}
