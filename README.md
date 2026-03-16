# 🏨 Hotel Booking System

A WPF desktop application built with **C# / .NET 8** that simulates a hotel booking workflow while demonstrating **eight GoF design patterns** across two labs — five creational (Lab 3) and three structural (Lab 4).

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
│
├── Adapter/                            # Lab 4 — Adapter pattern
│   ├── IPaymentService.cs              # Target interface (our system expects)
│   ├── StripePaymentGateway.cs         # Adaptee (incompatible external class)
│   └── StripePaymentAdapter.cs         # Adapter (translates IPaymentService → Stripe)
│
├── Builders/                           # Lab 3 — Builder pattern
│   ├── IBookingBuilder.cs
│   ├── BookingBuilder.cs
│   ├── BookingDirector.cs
│   └── BookingRequest.cs
│
├── Commands/
│   └── RelayCommand.cs                 # ICommand implementation for MVVM bindings
│
├── Composite/                          # Lab 4 — Composite pattern
│   ├── RoomServiceComponent.cs         # Abstract component (leaf + composite share this)
│   ├── RoomServiceItem.cs              # Leaf (single service item)
│   ├── RoomServicePackage.cs           # Composite (holds children, applies discount)
│   └── RoomServiceCatalog.cs           # Pre-built catalog with nested packages
│
├── Converters/
│   ├── BookingDisplayConverter.cs
│   ├── BoolToVisibilityConverter.cs
│   ├── PriceFormatConverter.cs
│   ├── RoomServiceConverters.cs        # Lab 4 — PKG/LEAF badge + price converters
│   └── StatusToColorConverter.cs
│
├── Facade/                             # Lab 4 — Façade pattern
│   ├── HotelFacade.cs                  # Façade (CheckInGuest, CheckOutGuest, GetBookingSummary)
│   └── FacadeResults.cs                # Result DTOs (CheckInResult, CheckOutResult)
│
├── Factories/                          # Lab 3 — Abstract Factory + Factory Method
│   ├── Booking/
│   │   ├── BookingFactoryProvider.cs
│   │   ├── StandardBookingFactory.cs
│   │   ├── PremiumBookingFactory.cs
│   │   ├── VipBookingFactory.cs
│   │   ├── Confirmation/
│   │   │   ├── StandardConfirmationHandler.cs
│   │   │   ├── PremiumConfirmationHandler.cs
│   │   │   └── VipConfirmationHandler.cs
│   │   └── Pricing/
│   │       ├── StandardPricingStrategy.cs
│   │       ├── PremiumPricingStrategy.cs
│   │       └── VipPricingStrategy.cs
│   └── Room/
│       ├── RoomCreator.cs              # Abstract creator (factory method lives here)
│       ├── StandardRoomCreator.cs
│       ├── DeluxeRoomCreator.cs
│       ├── SuiteRoomCreator.cs
│       └── RoomCreatorProvider.cs
│
├── Interfaces/
│   ├── Booking/
│   │   ├── IBookingConfirmationService.cs
│   │   ├── IBookingDurationCalculator.cs
│   │   ├── IBookingFactory.cs
│   │   ├── IBookingRepository.cs
│   │   ├── IBookingService.cs
│   │   ├── IConfirmationHandler.cs
│   │   └── IPricingStrategy.cs
│   ├── Room/
│   │   ├── IRoomPricingService.cs
│   │   └── IRoomProduct.cs
│   └── Shared/
│       ├── ILogger.cs
│       ├── IRoomRepository.cs
│       ├── IUserRepository.cs
│       └── IUserValidator.cs
│
├── Models/
│   ├── Booking/
│   │   ├── Booking.cs
│   │   ├── BookingResult.cs
│   │   └── Enums.cs
│   ├── Room/
│   │   ├── Room.cs
│   │   ├── StandardRoom.cs
│   │   ├── DeluxeRoom.cs
│   │   └── Suite.cs
│   └── User/
│       ├── User.cs
│       ├── Guest.cs
│       └── Admin.cs
│
├── Prototype/                          # Lab 3 — Prototype pattern
│   ├── IPrototype.cs
│   ├── RoomPrototypes.cs               # StandardRoomPrototype, DeluxeRoomPrototype, SuitePrototype
│   └── RoomPrototypeRegistry.cs
│
├── Services/
│   ├── Booking/
│   │   ├── BookingConfirmationService.cs
│   │   ├── BookingDurationCalculator.cs
│   │   ├── BookingService.cs
│   │   └── InMemoryBookingRepository.cs
│   ├── Room/
│   │   ├── InMemoryRoomRepository.cs
│   │   └── RoomPricingService.cs
│   └── User/
│       ├── ConsoleLogger.cs
│       ├── InMemoryUserRepository.cs
│       └── UserValidator.cs
│
├── Singleton/                          # Lab 3 — Singleton pattern
│   └── HotelAuditLogger.cs
│
├── ViewModels/
│   ├── BaseViewModel.cs                # INotifyPropertyChanged base
│   ├── BookingController.cs
│   ├── FacadeController.cs             # Lab 4 — Façade demo VM
│   ├── GuestController.cs
│   ├── LogController.cs                # Activity log (capped at 300 lines)
│   ├── MainViewModel.cs                # Root VM — wires all controllers + commands
│   ├── PaymentController.cs            # Lab 4 — Adapter demo VM
│   ├── RoomController.cs
│   └── RoomServiceController.cs        # Lab 4 — Composite demo VM
│
├── Views/
│   ├── FacadeView.xaml / .cs           # Lab 4 — Hotel Ops page
│   ├── PaymentView.xaml / .cs          # Lab 4 — Payment page
│   └── RoomServiceView.xaml / .cs      # Lab 4 — Room Services page
│
├── Lab2_AbstractFactory.puml
├── Lab2_FactoryMethod.puml
├── Lab3_Builder.puml
├── Lab3_Prototype.puml
├── Lab3_Singleton.puml
├── Lab4_Adapter.puml
├── Lab4_Composite.puml
├── Lab4_Facade.puml
│
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── AssemblyInfo.cs
└── HotelBookingSystem.csproj
```

---

## Implemented GoF Patterns

### Lab 3 — Creational Patterns

---

#### 1. Singleton — `HotelAuditLogger`

**Problem:** The application needs a single audit logger that every part of the system writes to. Multiple logger instances would scatter log output and break consistency.

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

**Proof it works** — `MainViewModel` obtains two references and compares them:

```csharp
var logger  = HotelAuditLogger.Instance;
var logger2 = HotelAuditLogger.Instance;
SingletonInfo = $"Singleton: same instance = {ReferenceEquals(logger, logger2)}";
// → "Singleton: same instance = True"
```

The result is displayed live in the sidebar.

---

#### 2. Prototype — `RoomPrototype` + `RoomPrototypeRegistry`

**Problem:** Each room type (Standard, Deluxe, Suite) has pre-configured defaults — price, capacity, amenities. Re-entering them manually every time is error-prone and repetitive.

**Solution:** Pre-configured template objects live in a registry. When a room type is selected, the registry returns a **clone** of the template — never the original. The clone is customized (room number, adjusted price) without affecting the template.

```csharp
public class DeluxeRoomPrototype : IPrototype<DeluxeRoomPrototype>
{
    public List<string> Amenities { get; set; }

    public DeluxeRoomPrototype Clone() =>
        new DeluxeRoomPrototype(RoomNumber, BasePrice, Capacity, HasBalcony,
                                new List<string>(Amenities))  // deep copy
        {
            IsAvailable = this.IsAvailable
        };
}
```

The registry always returns clones — the originals are never exposed:

```csharp
public RoomTemplateSnapshot GetClone(string key)
{
    if (!_registry.TryGetValue(key, out var cloneFunc))
        throw new KeyNotFoundException($"No prototype registered for: '{key}'");
    return cloneFunc();
}
```

**Where it's used:** in `RoomController`, changing the room type dropdown calls `GetClone()` and auto-fills price and capacity.

---

#### 3. Builder — `BookingBuilder` + `BookingDirector`

**Problem:** `BookingRequest` has 9 fields, many optional. Passing them all as constructor arguments leads to the telescoping constructor problem.

**Solution:** `BookingBuilder` assembles the request field by field through a fluent API. `BookingDirector` provides three preset construction sequences — Standard, Premium, VIP — so callers don't need to know which fields each type requires.

```csharp
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
```

After `GetResult()` the builder resets automatically, so the same director instance is safe to reuse.

---

#### 4. Factory Method — `RoomCreator`

**Problem:** `RoomController` originally contained a `switch` statement with `new StandardRoom(...)`, `new DeluxeRoom(...)`, `new Suite(...)`. Adding a new room type required modifying the controller — violating OCP.

**Solution:** Abstract `RoomCreator` defines the factory method `CreateProduct()`. Each concrete creator knows how to build its specific room type. `RoomCreatorProvider` selects the right creator at runtime by type name.

```
RoomCreator (abstract)
    └── CreateProduct(id, number, price, capacity) : IRoomProduct  ← factory method

StandardRoomCreator → StandardRoom
DeluxeRoomCreator   → DeluxeRoom
SuiteRoomCreator    → Suite
```

```csharp
public sealed class DeluxeRoomCreator : RoomCreator
{
    public override IRoomProduct CreateProduct(
        string roomId, string roomNumber, decimal basePrice, int capacity)
        => new DeluxeRoom(roomId, roomNumber, basePrice, capacity,
                          new List<string> { "Minibar", "Balcony", "Sea View" }, hasBalcony: true);
}
```

---

#### 5. Abstract Factory — `IBookingFactory`

**Problem:** Standard, Premium, and VIP bookings each need their own pricing logic and confirmation messages. These objects must always match — a VIP confirmation must never use standard pricing. Creating them independently risks mismatches.

**Solution:** `IBookingFactory` groups three related products (`Booking`, `IPricingStrategy`, `IConfirmationHandler`) into one interface. Each concrete factory creates a consistent, pre-matched family.

```csharp
public class VipBookingFactory : IBookingFactory
{
    public Booking              CreateBooking(...)           => new Booking(..., "VIP");
    public IPricingStrategy     CreatePricingStrategy()     => new VipPricingStrategy();
    public IConfirmationHandler CreateConfirmationHandler() => new VipConfirmationHandler();
}
```

| Factory | Pricing | Confirmation |
|---|---|---|
| `StandardBookingFactory` | Base rate × nights | Basic receipt |
| `PremiumBookingFactory` | 10% discount | + Early check-in, late check-out |
| `VipBookingFactory` | 20% off + 1 free night every 5 nights | + Spa, airport transfer, minibar |

---

### Lab 4 — Structural Patterns

---

#### 6. Adapter — `StripePaymentAdapter`

**Problem:** `StripePaymentGateway` (an external payment library) has an incompatible interface — it takes `double amountInCents`, a `string cardToken`, and a `string currencyCode`. Our system expects `IPaymentService` — `ProcessPayment(string guestId, decimal amount)`. Neither class can be modified.

**Solution:** `StripePaymentAdapter` implements `IPaymentService` and holds a `StripePaymentGateway` internally. It translates every call — converting `decimal` → `double cents`, treating `guestId` as the card token, and injecting `"USD"` as the currency — without either the client or the adaptee knowing about each other.

```csharp
// Target interface — what our system understands
public interface IPaymentService
{
    bool ProcessPayment(string guestId, decimal amount);
    bool RefundPayment(string guestId, decimal amount);
    string GetLastTransactionId();
}

// Adaptee — incompatible external class (cannot be changed)
public class StripePaymentGateway
{
    public bool ChargeCard(string cardToken, double amountInCents, string currencyCode) { ... }
    public bool RefundCharge(string chargeId, double amountInCents) { ... }
    public string GetLastChargeId() { ... }
}

// Adapter — bridges the two
public class StripePaymentAdapter : IPaymentService
{
    private readonly StripePaymentGateway _stripe;
    private string _lastTransactionId = string.Empty;

    public StripePaymentAdapter(StripePaymentGateway stripe) => _stripe = stripe;

    public bool ProcessPayment(string guestId, decimal amount)
    {
        bool success = _stripe.ChargeCard(guestId, (double)(amount * 100), "USD");
        if (success) _lastTransactionId = _stripe.GetLastChargeId();
        return success;
    }

    public bool RefundPayment(string guestId, decimal amount)
        => _stripe.RefundCharge(_lastTransactionId, (double)(amount * 100));

    public string GetLastTransactionId() => _lastTransactionId;
}
```

**Where it's used:** `HotelFacade` charges room costs at check-in and room services at check-out. `PaymentController` exposes manual charge/refund on the Payment page. In both cases the caller only ever sees `IPaymentService` — the Stripe-specific types are invisible.

**Interface translation diagram:**

```
PaymentController / HotelFacade
        │
        │  IPaymentService
        │  ProcessPayment(guestId, decimal amount)
        ▼
StripePaymentAdapter
        │
        │  translates:
        │  guestId       →  cardToken
        │  decimal       →  double cents  (× 100)
        │  (implicit)    →  "USD"
        ▼
StripePaymentGateway
        ChargeCard(cardToken, amountInCents, currencyCode)
```

---

#### 7. Composite — `RoomServiceComponent`

**Problem:** The room services catalog mixes individual items (a massage, a sandwich) with bundles that group several items together and apply a discount. Some bundles even contain other bundles. Code that processes orders shouldn't need `if (item is Package)` type checks to calculate totals.

**Solution:** `RoomServiceComponent` is the abstract component. `RoomServiceItem` is the leaf — it holds a single price. `RoomServicePackage` is the composite — it holds a list of children (items or other packages) and calculates its price by recursively summing them, then applying an optional discount. The client calls `GetPrice()` on any node and gets the correct total with no type checks.

```csharp
// Abstract component — leaf and composite share this contract
public abstract class RoomServiceComponent
{
    public abstract string Name { get; }
    public abstract decimal GetPrice();
    public abstract string GetDescription();
    public virtual void Add(RoomServiceComponent c) => throw new InvalidOperationException();
    public virtual void Remove(RoomServiceComponent c) => throw new InvalidOperationException();
}

// Leaf — no children, fixed price
public class RoomServiceItem : RoomServiceComponent
{
    private readonly decimal _price;
    public override decimal GetPrice() => _price;
}

// Composite — sums children recursively, applies discount
public class RoomServicePackage : RoomServiceComponent
{
    private readonly List<RoomServiceComponent> _children = new();
    private readonly decimal _discountPercent;

    public override void Add(RoomServiceComponent c) => _children.Add(c);

    public override decimal GetPrice()
    {
        decimal total = 0;
        foreach (var child in _children)
            total += child.GetPrice();          // works for both items and nested packages
        return total * (1 - _discountPercent / 100);
    }
}
```

**Catalog structure — two levels of nesting:**

```
VIP Welcome Bundle (20% off)           ← RoomServicePackage
├── Breakfast Package (10% off)        ← RoomServicePackage (nested composite)
│   ├── Continental Breakfast $18      ← RoomServiceItem (leaf)
│   └── Fresh Fruit Plate $12          ← RoomServiceItem (leaf)
├── Welcome Champagne $60              ← RoomServiceItem (leaf)
└── Fresh Flowers $35                  ← RoomServiceItem (leaf)
```

Calling `GetPrice()` on the VIP bundle traverses the whole tree and returns the correctly discounted total — the client code is a single loop with no `if` statements.

**Where it's used:** `RoomServiceController` drives the Room Services page. `HotelFacade.CheckOutGuest()` receives the ordered items list and calls `GetPrice()` on each to total the room services bill, then charges via the Adapter.

---

#### 8. Façade — `HotelFacade`

**Problem:** Hotel check-in and check-out each involve multiple subsystems — booking repository, room repository, user repository, payment service, booking service, and logger. Coordinating all of them from a UI ViewModel creates tight coupling and exposes implementation details to the presentation layer.

**Solution:** `HotelFacade` provides three simple methods that each orchestrate the full workflow internally. The ViewModel calls one method; the Façade handles everything else.

```csharp
public class HotelFacade
{
    // 6 subsystems injected — caller never sees them
    private readonly IBookingService _bookingService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentService _paymentService;   // → Adapter
    private readonly ILogger _logger;                   // → Singleton

    public CheckInResult CheckInGuest(string bookingId)
    {
        // 1. Validate booking is Confirmed
        // 2. Find room + guest
        // 3. Charge room cost  →  IPaymentService  →  [Adapter]  →  Stripe
        // 4. Log              →  ILogger           →  [Singleton]
        // returns CheckInResult (success flag + guest name + room + tx ID)
    }

    public CheckOutResult CheckOutGuest(string bookingId,
        IReadOnlyList<RoomServiceComponent> services)
    {
        // 1. Find booking
        // 2. Sum services via GetPrice()  →  [Composite]
        // 3. Charge services total        →  IPaymentService  →  [Adapter]
        // 4. Release room (CancelBooking)
        // 5. Log                          →  [Singleton]
        // returns CheckOutResult (total charged + itemised lines)
    }

    public string GetBookingSummary(string bookingId) { ... }
}
```

**What the ViewModel sees vs what the Façade does:**

| ViewModel call | Façade orchestrates |
|---|---|
| `CheckInGuest(id)` | validate → find entities → charge via Adapter → log via Singleton |
| `CheckOutGuest(id, services)` | sum via Composite → charge via Adapter → release room → log via Singleton |
| `GetBookingSummary(id)` | query 3 repositories → format summary string |

**Patterns converging in `CheckOutGuest`:**
- **Composite** calculates each service total (`GetPrice()`)
- **Adapter** charges the total (`IPaymentService` → `StripePaymentGateway`)
- **Singleton** logs the operation (`HotelAuditLogger.Instance`)
- **Façade** coordinates all three behind a single method call

---

## Pattern Interaction Map

```
User selects room type
        │
        ▼
[PROTOTYPE] RoomPrototypeRegistry.GetClone()
        │  returns cloned template — price/capacity pre-filled
        ▼
[FACTORY METHOD] RoomCreator.CreateRoom()
        │  creates the Room domain object
        ▼
User fills dates + booking type
        │
        ▼
[BUILDER] BookingDirector.Build{Standard|Premium|Vip}()
        │  assembles BookingRequest step by step
        ▼
[ABSTRACT FACTORY] IBookingFactory.Create*()
        │  creates Booking + Pricing + Confirmation as a matched family
        ▼
[SINGLETON] HotelAuditLogger.Instance.Info()
        │  every step above is logged through the single logger instance
        ▼
Booking saved — status: Pending
        │
        ▼
User confirms booking on Bookings page
        │
        ▼  (optionally)
[COMPOSITE] RoomServiceCatalog / RoomServicePackage.GetPrice()
        │  guest orders services — packages nest recursively
        ▼
[FAÇADE] HotelFacade.CheckInGuest()
        │  validates + charges room cost
        ├──► [ADAPTER] StripePaymentAdapter.ProcessPayment()
        │       translates decimal/guestId → Stripe ChargeCard(token, cents, USD)
        └──► [SINGLETON] HotelAuditLogger.Instance.Info()
        ▼
[FAÇADE] HotelFacade.CheckOutGuest(services)
        ├──► [COMPOSITE] sums all service totals via GetPrice()
        ├──► [ADAPTER] charges services total via StripePaymentAdapter
        └──► [SINGLETON] logs checkout + releases room
```

---

## How to Run

1. Open `HotelBookingSystem.sln` in Visual Studio 2022
2. Set `HotelBookingSystem` as the startup project
3. Press `F5`

### Workflow

| Step | Page | Patterns activated |
|---|---|---|
| Register a guest | New Booking | — |
| Assign a room | New Booking | Prototype, Factory Method |
| Create a booking | New Booking | Builder, Abstract Factory |
| Confirm the booking | Bookings | — |
| Add room services | Room Services (Composite) | Composite |
| Charge a payment | Payment (Adapter) | Adapter |
| Check in / Check out | Hotel Ops (Façade) | Façade + Adapter + Composite + Singleton |
| Review all activations | Activity Log | all patterns prefixed |

The sidebar shows a live Singleton proof: `same instance = True`.  
The Activity Log prefixes every entry: `[Prototype]`, `[Builder]`, `[Abstract Factory]`, `[Factory Method]`, `[Singleton]`, `[Adapter]`, `[Composite]`, `[Facade]`.
