# 🏨 Grand Horizon Hotel — Property Management System

A full-featured WPF desktop application built with **C# 12 / .NET 8** that simulates a real hotel booking and operations workflow. The system covers the complete guest lifecycle — registration, room assignment, booking creation, payment processing, room services, check-in, check-out, reporting, and dynamic pricing — while demonstrating **thirteen GoF design patterns** across three labs.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 / .NET 8 |
| UI Framework | WPF (Windows Presentation Foundation) |
| Architecture | MVVM (Model-View-ViewModel) |
| UI Polish | MaterialDesignThemes 4.9 |
| Email | System.Net.Mail — Gmail SMTP with app password |
| Config | System.Text.Json — appsettings.json |
| Auth | Mock AuthenticationService + SecureString |

---

## Project Structure

```
HotelBookingSystem/
│
├── Adapter/                                      # Lab 4 — Adapter
│   ├── IPaymentService.cs                        # Target interface
│   ├── StripePaymentGateway.cs                   # Adaptee (external, incompatible)
│   └── StripePaymentAdapter.cs                   # Object adapter
│
├── Bridge/                                       # Lab 5 — Bridge
│   ├── IReportDelivery.cs                        # Implementation interface (delivery)
│   ├── HotelReports.cs                           # Abstraction hierarchy (format)
│   │   ├── HotelReport  (abstract)
│   │   ├── TextHotelReport
│   │   ├── HtmlHotelReport
│   │   └── CsvHotelReport
│   └── ReportDeliveries.cs                       # Implementation hierarchy
│       ├── LogDelivery
│       ├── FileDelivery    → Desktop\rapoarte
│       ├── EmailDelivery   → Gmail SMTP
│       └── FileAndEmailDelivery
│
├── Builders/                                     # Lab 3 — Builder
│   ├── IBookingBuilder.cs
│   ├── BookingBuilder.cs
│   ├── BookingDirector.cs                        # Standard / Premium / VIP presets
│   └── BookingRequest.cs                         # Constructed product
│
├── Commands/
│   └── RelayCommand.cs                           # ICommand for MVVM bindings
│
├── Composite/                                    # Lab 4 — Composite
│   ├── RoomServiceComponent.cs                   # Abstract component
│   ├── RoomServiceItem.cs                        # Leaf — single item, fixed price
│   ├── RoomServicePackage.cs                     # Composite — recursive GetPrice()
│   └── RoomServiceCatalog.cs                     # Pre-built two-level catalog
│
├── Config/
│   └── AppSettings.cs                            # Typed loader for appsettings.json
│
├── Converters/
│   ├── BookingDisplayConverter.cs
│   ├── BoolToVisibilityConverter.cs
│   ├── PriceFormatConverter.cs
│   ├── RoomServiceConverters.cs
│   ├── StatusToColorConverter.cs
│   ├── StringEqualConverter.cs                   # Lab 6 — RadioButton ↔ string binding
│   └── ToastConverters.cs
│
├── Decorator/                                    # Lab 5 — Decorator
│   ├── IBookingNotificationService.cs            # Component interface
│   ├── BookingNotificationService.cs             # Core concrete component
│   ├── BookingNotificationDecorator.cs           # Base decorator
│   ├── LoggingNotificationDecorator.cs           # Adds audit log entries
│   ├── EmailNotificationDecorator.cs             # Sends real Gmail emails
│   └── SmsNotificationDecorator.cs               # Simulates SMS gateway
│
├── Email/
│   └── GmailEmailService.cs                      # Real SMTP sender (SmtpClient)
│
├── Facade/                                       # Lab 4 — Facade
│   ├── HotelFacade.cs                            # CheckInGuest / CheckOutGuest / Summary
│   └── FacadeResults.cs                          # CheckInResult / CheckOutResult DTOs
│
├── Factories/                                    # Labs 3-4 — Abstract Factory + Factory Method
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
│   │       ├── PremiumPricingStrategy.cs         # 10% discount
│   │       └── VipPricingStrategy.cs             # 20% + 1 free night / 5
│   └── Room/
│       ├── RoomCreator.cs                        # Abstract creator
│       ├── StandardRoomCreator.cs
│       ├── DeluxeRoomCreator.cs
│       ├── SuiteRoomCreator.cs
│       └── RoomCreatorProvider.cs
│
├── Flyweight/                                    # Lab 5 — Flyweight
│   ├── IRoomAmenityFlyweight.cs
│   ├── RoomAmenityFlyweight.cs                   # Intrinsic state (shared)
│   ├── RoomAmenityFactory.cs                     # Cache — one object per amenity type
│   └── RoomAmenityRenderer.cs                    # Entries hold extrinsic state per room
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
│   │   └── Enums.cs (BookingStatus)
│   ├── Room/
│   │   ├── Room.cs  (abstract)
│   │   ├── StandardRoom.cs
│   │   ├── DeluxeRoom.cs
│   │   └── Suite.cs
│   └── User/
│       ├── User.cs  (abstract)
│       ├── Guest.cs
│       └── Admin.cs
│
├── Prototype/                                    # Lab 3 — Prototype
│   ├── IPrototype.cs
│   ├── RoomPrototypes.cs                         # Standard/Deluxe/Suite templates
│   └── RoomPrototypeRegistry.cs                  # Registry — returns clones only
│
├── Proxy/                                        # Lab 5 — Proxy
│   ├── CachingRoomRepositoryProxy.cs             # Virtual proxy — 30s TTL cache
│   └── ProtectionRoomRepositoryProxy.cs          # Protection proxy — StaffRole checks
│
├── Services/
│   ├── AuthenticationService.cs
│   ├── IAuthService.cs
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
├── Singleton/                                    # Lab 3 — Singleton
│   └── HotelAuditLogger.cs                       # Lazy<T>, thread-safe, ILogger
│
├── Strategy/                                     # Lab 6 — Strategy
│   ├── IRoomPricingStrategy.cs                   # Strategy interface + PricingResult DTO
│   ├── ConcreteStrategies.cs                     # 6 concrete algorithms
│   │   ├── StandardRateStrategy                  # base × nights, no adjustment
│   │   ├── WeekendSurgeStrategy                  # Fri/Sat nights ×1.5 surcharge
│   │   ├── EarlyBirdStrategy                     # 30+ days: 20% off; 14-29: 10% off
│   │   ├── LastMinuteDealStrategy                # 0-3 days: 25% off; 4-7: 15% off
│   │   ├── LongStayStrategy                      # 7n:5%; 14n:10%; 21n:15%; 28n+:20%
│   │   └── SeasonalRateStrategy                  # Summer+40%, Winter+30%, Spring+5% per-night
│   └── RoomPricingCalculator.cs                  # Context — SetStrategy() / CalculatePrice()
│
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── BookingController.cs
│   ├── BridgeController.cs                       # Lab 5 async report generation
│   ├── DecoratorController.cs                    # Lab 5 runtime chain builder
│   ├── FacadeController.cs                       # Lab 4 CheckIn/CheckOut/Summary
│   ├── FlyweightController.cs                    # Lab 5 amenity entry loader
│   ├── GuestController.cs
│   ├── LogController.cs
│   ├── LoginViewModel.cs
│   ├── MainViewModel.cs                          # Root VM — wires all controllers
│   ├── PaymentController.cs
│   ├── ProxyController.cs                        # Lab 5 cache + auth proxy demos
│   ├── RoomController.cs
│   ├── RoomServiceController.cs
│   ├── StrategyController.cs                     # Lab 6 pricing calculator VM
│   └── ToastService.cs
│
├── Views/
│   ├── FacadeView.xaml / .cs
│   ├── LogView.xaml / .cs
│   ├── LoginView.xaml / .cs
│   ├── PatternsLab5View.xaml / .cs               # Tabbed: Flyweight/Decorator/Bridge/Proxy
│   ├── PaymentView.xaml / .cs
│   ├── RoomServiceView.xaml / .cs
│   └── StrategyView.xaml / .cs                   # Lab 6 — Pricing Calculator
│
├── Lab2_AbstractFactory.puml
├── Lab2_FactoryMethod.puml
├── Lab3_Builder.puml
├── Lab3_Prototype.puml
├── Lab3_Singleton.puml
├── Lab4_Adapter.puml
├── Lab4_Composite.puml
├── Lab4_Facade.puml
├── Lab6_Strategy.puml
│
├── appsettings.json
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

#### 2. Prototype — `RoomPrototypeRegistry`

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

### Lab 4 — Structural Patterns I

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

---

### Lab 5 — Structural Patterns II

---

#### 9. Flyweight — `RoomAmenityFlyweight`

**Problem:** The hotel has 200 rooms, and each can have up to 5 amenities (WiFi, Pool, Gym, etc.). Creating a new object for every amenity in every room would result in 1,000 duplicated `RoomAmenity` objects, wasting memory since intrinsic properties (icon, name, color) never change.

**Solution:** Intrinsic state (`AmenityType`, `Icon`, `Color`, `Category`) is stored once per unique amenity type in `RoomAmenityFlyweight`. `RoomAmenityFactory` caches these flyweights. Rooms store only references. The extrinsic state (`roomId`, `roomPrice`) is passed in when needed for rendering.

```csharp
public class RoomAmenityFlyweight : IRoomAmenityFlyweight
{
    // Intrinsic state (shared, immutable)
    public string AmenityType { get; }
    public string Icon { get; }
    public string Color { get; }
    public string Category { get; }

    public RoomAmenityFlyweight(string amenityType, string icon, string color, string category)
    {
        AmenityType = amenityType;
        Icon = icon;
        Color = color;
        Category = category;
    }

    // Operation — uses intrinsic state + extrinsic params
    public string Render(string roomId, decimal roomPrice)
    {
        return $"[{Icon} {AmenityType}] Room:{roomId} @${roomPrice:F0} ({Category})";
    }
}
```

---

#### 10. Decorator — `BookingNotificationDecorator`

**Problem:** When a booking is finalized, we need to send notifications. Standard booking logic shouldn't be cluttered with emails, SMS, and logging. Some guests want emails, others want SMS, others want both. Adding inherited subclasses for every combination of notification types would lead to class explosion.

**Solution:** Decorator pattern wraps notification services around each other dynamically at runtime. 
`BookingNotificationDecorator` implements `IBookingNotificationService` and forwards calls to an inner component. Concrete decorators (`EmailNotificationDecorator`, `LoggingNotificationDecorator`, `SmsNotificationDecorator`) add their specific behavior before or after delegating.

```csharp
public abstract class BookingNotificationDecorator : IBookingNotificationService
{
    protected readonly IBookingNotificationService _inner;

    protected BookingNotificationDecorator(IBookingNotificationService inner)
    {
        _inner = inner;
    }

    public virtual void NotifyBookingCreated(Booking booking)
        => _inner.NotifyBookingCreated(booking);
        
    // and other notification methods...
}
```

---

#### 11. Bridge — `HotelReport × IReportDelivery`

**Problem:** We need report generation. Reports have formats (Text, HTML, CSV) and deliveries (File, Email, Log, File+Email). Using inheritance would require `FileTextReport`, `EmailTextReport`, `FileHtmlReport`, etc., resulting in a M×N class explosion.

**Solution:** Bridge pattern separates the Abstraction (`HotelReport` handling the format) from the Implementation (`IReportDelivery` handling the sending mechanism). `HotelReport` has a reference to `IReportDelivery`.

```csharp
public abstract class HotelReport
{
    protected readonly IReportDelivery _delivery; // The Bridge

    protected HotelReport(IReportDelivery delivery) => _delivery = delivery;

    public async Task GenerateAsync(IReadOnlyList<Booking> bookings, 
                                    DateTime periodStart, DateTime periodEnd)
    {
        string title = GetTitle(periodStart, periodEnd);
        string content = FormatContent(bookings, periodStart, periodEnd);
        string filename = GetFilename(periodStart);

        await _delivery.DeliverAsync(content, title, filename); // BRIDGE call
    }

    protected abstract string FormatContent(IReadOnlyList<Booking> bookings, DateTime from, DateTime to); 
}
```

---

#### 12. Proxy — `CachingRoomRepositoryProxy`

**Problem:** Fetching all available rooms can be expensive if we query the repository repeatedly. Furthermore, we might need security checks without putting caching and authorization directly inside `RoomRepository` (violating SRP).

**Solution:** A Proxy class intercepts calls. `CachingRoomRepositoryProxy` implements `IRoomRepository` and holds a reference to the real repository. It caches `GetAvailableRooms()` for 30s, and intercepts `Save()` to invalidate the cache.

```csharp
public class CachingRoomRepositoryProxy : IRoomRepository
{
    private readonly IRoomRepository _real;
    private List<Room>? _availableRoomsCache;
    private DateTime _availExpires = DateTime.MinValue;

    public List<Room> GetAvailableRooms()
    {
        if (_availableRoomsCache != null && DateTime.UtcNow < _availExpires)
        {
            return new List<Room>(_availableRoomsCache); // Cache Hit
        }

        // Cache Miss
        _availableRoomsCache = _real.GetAvailableRooms();
        _availExpires = DateTime.UtcNow.AddSeconds(30);
        return new List<Room>(_availableRoomsCache);
    }
}
```

---

### Lab 6 — Behavioral Patterns I

---

#### 13. Strategy — `RoomPricingCalculator`

**Problem:** Without Strategy, the pricing algorithm had a huge `switch`/`if-else` statement to handle Weekend Surge, Seasonal Rates, Early Bird deals, etc. Adding a new pricing rule required modifying the calculator class, which violates the Open/Closed Principle.

**Solution:** We extracted pricing rules into 6 distinct classes that implement `IRoomPricingStrategy`. The `RoomPricingCalculator` (Context) holds an `IRoomPricingStrategy` and delegates the actual logic to it.

```csharp
public sealed class RoomPricingCalculator
{
    private IRoomPricingStrategy _strategy;

    public RoomPricingCalculator(IRoomPricingStrategy initialStrategy)
    {
        _strategy = initialStrategy;
    }

    // Swap strategy at runtime — zero other code changes needed
    public void SetStrategy(IRoomPricingStrategy strategy)
    {
        _strategy = strategy;
    }

    public PricingResult CalculatePrice(decimal basePrice, DateTime checkIn, DateTime checkOut)
    {
        // ALL pricing logic lives in _strategy, not here
        return _strategy.Calculate(basePrice, checkIn, checkOut);
    }
}
```

**All 5 SOLID principles respected:**

| Principle | Explanation |
|---|---|
| **S** | Each ConcreteStrategy owns one algorithm. Context owns zero pricing logic. |
| **O** | Add strategy 7 with zero changes to Context or any existing class. |
| **L** | Any ConcreteStrategy substitutes IRoomPricingStrategy without breaking Context. |
| **I** | IRoomPricingStrategy declares exactly one method: Calculate(). |
| **D** | Context depends on IRoomPricingStrategy (abstraction), never on concrete strategies. |

**Pricing Calculator UI features:**
- Base price input + date pickers + live nights counter
- Live strategy badge (coloured) showing the active algorithm
- 6 radio-button strategy selector — click to swap at runtime
- Step-by-step breakdown terminal (per-night breakdown for Seasonal / Weekend Surge)
- All 6 strategies evaluated simultaneously in a comparison table
- ACTIVE badge on current row, BEST badge on cheapest row
- Smart tip: "You could save $X by switching to X strategy"

---

## Configuration

### appsettings.json
```json
{
  "GmailDefaults": {
    "Email": "your@gmail.com",
    "AppPassword": "xxxx xxxx xxxx xxxx",
    "DisplayName": "Grand Horizon Hotel PMS"
  },
  "ReportSettings": {
    "OutputDirectory": "C:\\Users\\yourname\\Desktop\\rapoarte"
  }
}
```

### Demo login credentials
| Username | Password |
|---|---|
| `admin` | `admin` |
| `staff` | `staff` |

---

## How to Run

```
1. Open HotelBookingSystem.sln in Visual Studio 2022
2. Confirm appsettings.json is in HotelBookingSystem/
3. Set HotelBookingSystem as startup project
4. Press F5
```

### Full workflow

| Step | Page | Patterns |
|---|---|---|
| Sign in | Login | — |
| Register guest | New Booking | Decorator (welcome email) |
| Assign room | New Booking | Prototype, Factory Method |
| Create booking | New Booking | Builder, Abstract Factory, Decorator |
| Confirm booking | All Bookings | — |
| Add room services | Room Services | Composite |
| Charge payment | Payments | Adapter |
| Check in | Hotel Ops | Facade + Adapter + Singleton |
| Check out | Hotel Ops | Facade + Composite + Adapter + Singleton |
| Generate report | Lab 5 → Bridge | Bridge (File/Email/Both) |
| Calculate pricing | Lab 6 → Pricing Strategy | Strategy (6 algorithms) |
| Review all events | Activity Log | All 13 patterns prefixed |

The sidebar shows a live Singleton proof: `same instance = True`.  
The Activity Log prefixes every entry: `[Prototype]`, `[Builder]`, `[Abstract Factory]`, `[Factory Method]`, `[Singleton]`, `[Adapter]`, `[Composite]`, `[Facade]`.

## Configuration

### appsettings.json
```json
{
  "GmailDefaults": {
    "Email": "your@gmail.com",
    "AppPassword": "xxxx xxxx xxxx xxxx",
    "DisplayName": "Grand Horizon Hotel PMS"
  },
  "ReportSettings": {
    "OutputDirectory": "C:\\Users\\yourname\\Desktop\\rapoarte"
  }
}
```

### Demo login credentials
| Username | Password |
|---|---|
| `admin` | `admin` |
| `staff` | `staff` |

---

## GitHub Actions Release

```bash
git tag v1.0.0 && git push origin v1.0.0
```

Triggers `.github/workflows/release.yml` → builds → self-contained `win-x64` publish → ZIP → GitHub Release.