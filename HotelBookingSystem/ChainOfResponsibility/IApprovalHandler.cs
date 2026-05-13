using System;

namespace HotelBookingSystem.ChainOfResponsibility
{
    public record BookingProcessRequest(
        string GuestId, 
        string RoomId, 
        DateTime CheckIn, 
        DateTime CheckOut, 
        decimal ExpectedPrice);

    public record BookingProcessResult(
        bool Success, 
        string ProcessorName, 
        string Message, 
        string StatusColor = "#EF4444");

    public interface IBookingHandler
    {
        IBookingHandler SetNext(IBookingHandler next);
        BookingProcessResult Handle(BookingProcessRequest request);
    }
}
