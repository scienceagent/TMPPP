#### 15. Chain of Responsibility — `BookingHandler` pipeline

**Problem:** A booking request must satisfy multiple independent business rules (guest eligibility, room availability, minimum stay policies, and pricing verification). Hard-coding these checks in a single service makes it difficult to maintain or reorder them.

**Solution:** Implement a validation pipeline using the Chain of Responsibility. Each business rule is encapsulated in its own handler. The booking request flows through the chain until it is either rejected by a handler or successfully passes all stages.

```csharp
public abstract class BaseBookingHandler : IBookingHandler
{
    private IBookingHandler? _next;

    public IBookingHandler SetNext(IBookingHandler next)
    {
        _next = next;
        return next;
    }

    protected BookingProcessResult PassToNext(BookingProcessRequest request)
    {
        if (_next != null) return _next.Handle(request);
        return new BookingProcessResult(true, "System", "All checks passed.");
    }
}
```

```csharp
public class StayDurationHandler : BaseBookingHandler
{
    public override BookingProcessResult Handle(BookingProcessRequest request)
    {
        var duration = (request.CheckOut - request.CheckIn).TotalDays;
        if (duration < 1)
            return new BookingProcessResult(false, "Policy", "Min stay 1 night.");
        return PassToNext(request);
    }
}
```

---

#### 16. State — `BookingStatus` lifecycle

**Problem:** A booking goes through various stages (Created → Confirmed → CheckedIn → CheckedOut). Each stage allows different operations (e.g., you can't cancel after check-in). Managing this with `switch` statements becomes unmanageable as complexity grows.

**Solution:** Use the State pattern. Each status is a separate class implementing `IBookingState`. The `BookingContext` simply delegates actions to its current state object, which knows which transitions are valid.

```csharp
public class ConfirmedState : IBookingState
{
    public void CheckIn(BookingContext context) 
        => context.TransitionTo(new CheckedInState());

    public void Cancel(BookingContext context) 
        => context.TransitionTo(new CancelledState());
}
```

```csharp
public class CheckedInState : IBookingState
{
    public void Cancel(BookingContext context) 
        => throw new InvalidOperationException("Cannot cancel after check-in.");
}
```
