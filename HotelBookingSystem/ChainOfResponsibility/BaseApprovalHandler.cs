using System;

namespace HotelBookingSystem.ChainOfResponsibility
{
    public abstract class BaseBookingHandler : IBookingHandler
    {
        private IBookingHandler? _next;

        public IBookingHandler SetNext(IBookingHandler next)
        {
            _next = next;
            return next;
        }

        public abstract BookingProcessResult Handle(BookingProcessRequest request);

        protected BookingProcessResult PassToNext(BookingProcessRequest request)
        {
            if (_next != null)
                return _next.Handle(request);

            return new BookingProcessResult(true, "System", 
                "All validation steps passed. Booking request is ready for finalization.", "#10B981");
        }
    }
}
