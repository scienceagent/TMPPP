# 🏨 Hotel Booking System

A WPF desktop application built with **C# / .NET 8** that simulates a hotel booking workflow while demonstrating **five GoF creational design patterns** applied to a real domain.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 / .NET 8 |
| UI Framework | WPF (Windows Presentation Foundation) |
| Architecture | MVVM |
| UI Library | MaterialDesignThemes |
| Pattern | Dependency Inversion via constructor injection |

---

## Project Structure

```
HotelBookingSystem/
├── Builders/                   # Builder pattern
│   ├── IBookingBuilder.cs
│   ├── BookingBuilder.cs
│   ├── BookingDirector.cs
│   └── BookingRequest.cs
├── Prototype/                  # Prototype pattern
│   ├── RoomPrototype.cs
│   └── RoomPrototypeRegistry.cs
├── Singleton/                  # Singleton pattern
│   └── HotelAuditLogger.cs
├── Factories/                  # Abstract Factory + Factory Method
│   ├── Booking/
│   │   ├── IBookingFactory.cs
│   │   ├── StandardBookingFactory.cs
│   │   ├── PremiumBookingFactory.cs
│   │   ├── VipBookingFactory.cs
│   │   ├── BookingFactoryProvider.cs
│   │   ├── Pricing/
│   │   └── Confirmation/
│   └── Room/
│       ├── RoomCreator.cs
│       ├── StandardRoomCreator.cs
│       ├── DeluxeRoomCreator.cs
│       ├── SuiteRoomCreator.cs
│       └── RoomCreatorProvider.cs
├── Models/                     # Domain entities
│   ├── Room/
│   ├── Booking/
│   └── User/
├── Interfaces/                 # Abstraction layer
├── Services/                   # Business logic
├── ViewModels/                 # MVVM controllers
├── Views/                      # UserControl pages
│   ├── Styles.xaml
│   ├── NewBookingView.xaml
│   ├── BookingsView.xaml
│   └── ActivityLogView.xaml
├── Converters/
└── Commands/
```

---

## Implemented GoF Creational Patterns

### 1. Singleton — `HotelAuditLogger`

**Problem:** The application needs a single audit logger that every part of the system writes to. Creating multiple logger instances would scatter log output and break consistency.

**Solution:** `HotelAuditLogger` uses `Lazy<T>` for thread-safe, on-demand initialization. The private constructor blocks any external instantiation.

```csharp
public sealed class HotelAuditLogger : ILogger
{
    private static readonly Lazy<HotelAuditLogger> _instance =
        new Lazy<HotelAuditLogger>(() => new HotelAuditLogger());

    public static HotelAuditLogger Instance => _instance.Value;

    private HotelAuditLogger() { }

    public void Info(string message)  => Console.WriteLine($"{DateTime.Now:HH:mm:ss} [INFO]  {message}");
    public void Warn(string message)  => Console.WriteLine($"{DateTime.Now:HH:mm:ss} [WARN]  {message}");
    public void Error(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss} [ERROR] {message}");
}
```

**Proof it works** — in `MainViewModel`, two references are obtained and compared:

```csharp
var logger  = HotelAuditLogger.Instance;
var logger2 = HotelAuditLogger.Instance;
SingletonInfo = $"Singleton: same instance = {ReferenceEquals(logger, logger2)}";
// → "Singleton: same instance = True"
```

This result is displayed live in the sidebar of the application.

---

### 2. Prototype — `RoomPrototype` + `RoomPrototypeRegistry`

**Problem:** Each room type (Standard, Deluxe, Suite) has a set of pre-configured defaults — price, capacity, amenities. Re-entering these values manually every time a new room is created is error-prone and repetitive.

**Solution:** Pre-configured template objects are stored in a registry. When a room type is selected, the registry returns a **clone** of the template, never the original. The clone can then be customized (room number, adjusted price) without affecting the template.

```csharp
// Abstract prototype — every subclass must implement Clone()
public abstract class RoomPrototype
{
    public string  RoomNumber { get; set; }
    public decimal BasePrice  { get; set; }
    public abstract int Capacity { get; set; }
    public abstract RoomPrototype Clone();
}

// Concrete prototype — deep copies the Amenities list
public class DeluxeRoomPrototype : RoomPrototype
{
    public List<string> Amenities { get; set; } = new();

    public override RoomPrototype Clone() =>
        new DeluxeRoomPrototype
        {
            RoomNumber = this.RoomNumber,
            BasePrice  = this.BasePrice,
            Capacity   = this.Capacity,
            Amenities  = new List<string>(this.Amenities)  // deep copy
        };
}
```

The registry holds the three templates and always returns clones:

```csharp
public RoomPrototype GetClone(string key)
{
    if (!_prototypes.TryGetValue(key, out var prototype))
        throw new KeyNotFoundException($"No prototype registered for: '{key}'");

    return prototype.Clone();  // original is never exposed
}
```

**Where it's used:** in `RoomController`, when the user changes the room type dropdown, `GetClone()` is called and the price/capacity fields auto-fill with the template values.

---

### 3. Builder — `BookingBuilder` + `BookingDirector`

**Problem:** A `BookingRequest` has 9 fields, many optional (breakfast, airport transfer, special request). Passing them all as constructor parameters leads to the classic *telescoping constructor* problem — hard to read and easy to get wrong.

**Solution:** The `BookingBuilder` assembles the request field by field through a fluent API. The `BookingDirector` provides three preset construction sequences so the caller doesn't need to know which fields are required for each booking type.

```csharp
// Builder interface
public interface IBookingBuilder
{
    IBookingBuilder SetGuest(string guestId);
    IBookingBuilder SetRoom(string roomId);
    IBookingBuilder SetDates(DateTime checkIn, DateTime checkOut);
    IBookingBuilder SetBookingType(string type);
    IBookingBuilder WithBreakfast();
    IBookingBuilder WithAirportTransfer();
    IBookingBuilder WithSpecialRequest(string note);
    BookingRequest  GetResult();
}

// Director — knows the correct sequence for each booking type
public class BookingDirector
{
    private readonly IBookingBuilder _builder;

    public BookingDirector(IBookingBuilder builder) => _builder = builder;

    public BookingRequest BuildVip(string guestId, string roomId,
        DateTime checkIn, DateTime checkOut) =>
        _builder
            .SetGuest(guestId)
            .SetRoom(roomId)
            .SetDates(checkIn, checkOut)
            .SetBookingType("VIP")
            .WithBreakfast()
            .WithAirportTransfer()
            .WithSpecialRequest("VIP welcome package")
            .GetResult();
}
```

After `GetResult()` is called, the builder resets its internal state automatically, so the same director instance can be reused for subsequent bookings without stale data bleeding through.

---

### 4. Factory Method — `RoomCreator`

**Problem:** `RoomController` originally had a `switch` statement with `new StandardRoom(...)`, `new DeluxeRoom(...)`, `new Suite(...)`. Adding a new room type meant modifying the controller — violating OCP.

**Solution:** An abstract `RoomCreator` defines the factory method `CreateProduct()`. Each concrete creator (`StandardRoomCreator`, `DeluxeRoomCreator`, `SuiteRoomCreator`) knows how to build its specific room type. `RoomCreatorProvider` selects the correct creator at runtime by type name.

```
RoomCreator (abstract)
    └── CreateProduct(id, number, price, capacity) : IRoomProduct  ← factory method

StandardRoomCreator → StandardRoom
DeluxeRoomCreator   → DeluxeRoom
SuiteRoomCreator    → Suite
```

```csharp
// Concrete creator
public sealed class DeluxeRoomCreator : RoomCreator
{
    public override IRoomProduct CreateProduct(
        string roomId, string roomNumber, decimal basePrice, int capacity)
        => new DeluxeRoom(roomId, roomNumber, basePrice, capacity,
                          new List<string> { "Minibar", "Balcony", "Sea View" }, hasBalcony: true);
}
```

**Key difference from Abstract Factory:** Factory Method creates **one product**. Abstract Factory creates a **family of related products** — see below.

---

### 5. Abstract Factory — `IBookingFactory`

**Problem:** Standard, Premium, and VIP bookings need different pricing logic *and* different confirmation messages. These objects must always match — a VIP confirmation message must use VIP pricing, never standard pricing. Creating them independently risks mismatches.

**Solution:** `IBookingFactory` groups the three related products (`Booking`, `IPricingStrategy`, `IConfirmationHandler`) into a single interface. Each concrete factory creates a consistent family.

```csharp
public interface IBookingFactory
{
    Booking              CreateBooking(...);
    IPricingStrategy     CreatePricingStrategy();
    IConfirmationHandler CreateConfirmationHandler();
}

// VIP family — all three objects belong together
public class VipBookingFactory : IBookingFactory
{
    public Booking              CreateBooking(...)           => new Booking(..., "VIP");
    public IPricingStrategy     CreatePricingStrategy()     => new VipPricingStrategy();
    public IConfirmationHandler CreateConfirmationHandler() => new VipConfirmationHandler();
}
```

| Factory | Pricing | Confirmation |
|---|---|---|
| `StandardBookingFactory` | Base rate × nights | Basic confirmation |
| `PremiumBookingFactory` | 10% discount | + Early check-in, late check-out |
| `VipBookingFactory` | 20% off + 1 free night / 5 nights | + Spa, airport transfer, minibar |

In `BookingController`, the Builder builds the request first, then the Abstract Factory creates the consistent product family:

```csharp
// Step 1 — Builder assembles the request
var request = _director.BuildVip(user.Id, room.RoomId, checkIn, checkOut);

// Step 2 — Abstract Factory creates matching Booking + Pricing + Confirmation
var factory      = _factoryProvider.GetFactory("VIP");
var booking      = factory.CreateBooking(...);
var pricing      = factory.CreatePricingStrategy();
var confirmation = factory.CreateConfirmationHandler();
```

---

## Pattern Interaction Map

```
User selects room type
        │
        ▼
[PROTOTYPE] RoomPrototypeRegistry.GetClone()
        │  returns cloned template with default price/capacity
        ▼
[FACTORY METHOD] RoomCreator.CreateRoom()
        │  creates the actual Room domain object
        ▼
User fills dates and booking type
        │
        ▼
[BUILDER] BookingDirector.BuildVip/Premium/Standard()
        │  assembles BookingRequest step by step
        ▼
[ABSTRACT FACTORY] IBookingFactory.Create*()
        │  creates Booking + Pricing + Confirmation as a matched family
        ▼
[SINGLETON] HotelAuditLogger.Instance.Info()
        │  every step above is logged through the single logger instance
        ▼
Booking saved to repository
```

---

## How to Run

1. Open `HotelBookingSystem.sln` in Visual Studio 2022
2. Set `HotelBookingSystem` as the startup project
3. Press `F5`

**Workflow inside the app:**
- **New Booking** — register a guest → assign a room → select booking type and dates → create
- **Bookings** — view all bookings, confirm or cancel selected
- **Activity Log** — see every pattern activation prefixed by `[Prototype]`, `[Builder]`, `[Abstract Factory]`, `[Factory Method]`

The sidebar shows a live Singleton proof: `same instance = True`.
