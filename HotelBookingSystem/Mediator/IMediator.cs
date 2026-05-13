using System;

namespace HotelBookingSystem.Mediator
{
    public interface IHotelMediator
    {
        void Notify(object sender, string @event, object? data = null);
    }

    public abstract class HotelComponent
    {
        protected IHotelMediator _mediator;

        public HotelComponent(IHotelMediator mediator)
        {
            _mediator = mediator;
        }

        public void SetMediator(IHotelMediator mediator)
        {
            _mediator = mediator;
        }
    }
}
