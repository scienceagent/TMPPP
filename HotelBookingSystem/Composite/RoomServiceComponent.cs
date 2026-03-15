namespace HotelBookingSystem.Composite
{
     public abstract class RoomServiceComponent
     {
          public abstract string Name { get; }
          public abstract decimal GetPrice();
          public abstract string GetDescription();
          public virtual void Add(RoomServiceComponent c) => throw new System.InvalidOperationException();
          public virtual void Remove(RoomServiceComponent c) => throw new System.InvalidOperationException();
     }
}