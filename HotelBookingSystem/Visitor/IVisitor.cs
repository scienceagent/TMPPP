using HotelBookingSystem.Models;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.Visitor
{
    public interface IVisitor
    {
        void Visit(Booking booking);
        void Visit(Room room);
        void Visit(User user);
    }

    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}
