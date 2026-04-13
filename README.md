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

**Solution:** Pre-configured template objects live in a registry. When a room type is selected, the registry returns a clone of the template instead of the original instance.

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

---

#### 3. Builder — `BookingBuilder` + `BookingDirector`

**Problem:** `BookingRequest` has 9 fields, many optional. Passing them all as constructor arguments leads to the telescoping constructor problem.

**Solution:** `BookingBuilder` assembles the request field by field through a fluent API. `BookingDirector` provides preset construction sequences so callers don't need to know specific field requirements.

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

---

#### 4. Factory Method — `RoomCreator`

**Problem:** `RoomController` originally contained a `switch` statement specifying concrete types (`new StandardRoom(...)`, etc.). Adding a new room type required modifying the controller, violating OCP.

**Solution:** Abstract `RoomCreator` defines the factory method `CreateProduct()`. Each concrete creator subclass knows how to build its specific room type.

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

**Problem:** Standard, Premium, and VIP bookings each need their own pricing logic and confirmation messages. These objects must always match — a VIP confirmation must never use standard pricing.

**Solution:** `IBookingFactory` groups related products (`Booking`, `IPricingStrategy`, `IConfirmationHandler`) into one interface to guarantee a matched family.

```csharp
public class VipBookingFactory : IBookingFactory
{
    public Booking              CreateBooking(...)           => new Booking(..., "VIP");
    public IPricingStrategy     CreatePricingStrategy()     => new VipPricingStrategy();
    public IConfirmationHandler CreateConfirmationHandler() => new VipConfirmationHandler();
}
```

---

### Lab 4 — Structural Patterns I

---

#### 6. Adapter — `StripePaymentAdapter`

**Problem:** `StripePaymentGateway` (an external payment library) has an incompatible interface. Our system expects `IPaymentService`, while the gateway needs token, cents, and currency inputs.

**Solution:** `StripePaymentAdapter` implements `IPaymentService` and internally translates requests before passing them to the external `StripePaymentGateway`.

```csharp
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
}
```

---

#### 7. Composite — `RoomServiceComponent`

**Problem:** The room services catalog mixes individual items with nested packages. Client code shouldn't need `if (item is Package)` runtime type checks to calculate the total bill.

**Solution:** Abstract `RoomServiceComponent`. Leaves return a fixed price, while composites recursively sum their children's prices before applying their own discounts.

```csharp
public class RoomServiceItem : RoomServiceComponent
{
    private readonly decimal _price;
    public override decimal GetPrice() => _price;
}

public class RoomServicePackage : RoomServiceComponent
{
    private readonly List<RoomServiceComponent> _children = new();
    private readonly decimal _discountPercent;

    public override void Add(RoomServiceComponent c) => _children.Add(c);

    public override decimal GetPrice()
    {
        decimal total = 0;
        foreach (var child in _children)
            total += child.GetPrice();
        return total * (1 - _discountPercent / 100);
    }
}
```

---

#### 8. Façade — `HotelFacade`

**Problem:** Hotel check-in and check-out orchestrate multiple subsystems (booking, rooms, payments, logs). Coordinating all of them directly from a ViewModel creates tight coupling.

**Solution:** Provide a unified interface (`HotelFacade`) that orchestrates the subsystems, reducing complexity and external dependencies for clients.

```csharp
public class HotelFacade
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentService _paymentService;
    private readonly ILogger _logger;

    public CheckOutResult CheckOutGuest(string bookingId, IReadOnlyList<RoomServiceComponent> services)
    {
        // 1. Find booking
        // 2. Sum services via Composite GetPrice()
        // 3. Charge via Adapter _paymentService
        // 4. Free room / update status
        // 5. Log via Singleton _logger
    }
}
```

---

### Lab 5 — Structural Patterns II

---

#### 9. Flyweight — `RoomAmenityFlyweight`

**Problem:** The hotel has 200 rooms, each up to 5 amenities. Creating full objects for every amenity in every room wastes memory on identical intrinsic properties (icon, category).

**Solution:** Extract and share invariant properties (intrinsic state) across a cached flyweight instance, while passing variable contextual parameters (extrinsic state) when rendering.

```csharp
public class RoomAmenityFlyweight : IRoomAmenityFlyweight
{
    public string AmenityType { get; }
    public string Icon { get; }
    public string Category { get; }

    public RoomAmenityFlyweight(string amenityType, string icon, string category)
    {
        AmenityType = amenityType;
        Icon = icon;
        Category = category;
    }

    public string Render(string roomId, decimal roomPrice)
    {
        return $"[{Icon} {AmenityType}] Room:{roomId} @${roomPrice:F0} ({Category})";
    }
}
```

---

#### 10. Decorator — `BookingNotificationDecorator`

**Problem:** Standard booking shouldn't be hard-coupled to emails, SMS, and logging. Adding subclasses for every combination of notification types yields a massive class explosion.

**Solution:** Dynamically wrap notification services at runtime. Decorator implements the component interface, wrapping the original component and adding extra behaviors (like email or SMS).

```csharp
public abstract class BookingNotificationDecorator : IBookingNotificationService
{
    protected readonly IBookingNotificationService _inner;

    protected BookingNotificationDecorator(IBookingNotificationService inner) => _inner = inner;

    public virtual void NotifyBookingCreated(Booking booking) => _inner.NotifyBookingCreated(booking);
}
```

---

#### 11. Bridge — `HotelReport × IReportDelivery`

**Problem:** Mixing report formats (Text, HTML, CSV) and target deliveries (File, Email, Both) generates an M×N class explosion tied up in rigid inheritance structures.

**Solution:** Separate the Abstraction (`HotelReport` handling formats) from the Implementation (`IReportDelivery` handling transfer mechanisms) using composition over inheritance.

```csharp
public abstract class HotelReport
{
    protected readonly IReportDelivery _delivery;

    protected HotelReport(IReportDelivery delivery) => _delivery = delivery;

    public async Task GenerateAsync(IReadOnlyList<Booking> bookings)
    {
        string content = FormatContent(bookings);
        await _delivery.DeliverAsync(content, "Report", "report.txt"); // Bridge Call
    }

    protected abstract string FormatContent(IReadOnlyList<Booking> bookings); 
}
```

---

#### 12. Proxy — `CachingRoomRepositoryProxy`

**Problem:** Reading availability via the repository repeatedly introduces heavy overhead. Directly adding caching to the core repository breaks the Single Responsibility Principle.

**Solution:** Create a surrogate Proxy class containing the exact same interface, but inject transparent caching logic before fetching real data from the actual repository object.

```csharp
public class CachingRoomRepositoryProxy : IRoomRepository
{
    private readonly IRoomRepository _real;
    private List<Room>? _availableRoomsCache;
    private DateTime _availExpires = DateTime.MinValue;

    public List<Room> GetAvailableRooms()
    {
        if (_availableRoomsCache != null && DateTime.UtcNow < _availExpires)
            return new List<Room>(_availableRoomsCache);

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

**Problem:** Adding multiple conditional modifiers (Weekend Surge, Seasonal, Early Bird) forces the calculator into one huge `if-else` block, severely breaking the Open/Closed Principle.

**Solution:** Abstract algorithms into their own explicit strategy classes. The Context merely hosts the current interface-driven strategy, delegating calculations entirely to the active algorithm block.

```csharp
public sealed class RoomPricingCalculator
{
    private IRoomPricingStrategy _strategy;

    public RoomPricingCalculator(IRoomPricingStrategy initialStrategy)
    {
        _strategy = initialStrategy;
    }

    public void SetStrategy(IRoomPricingStrategy strategy) => _strategy = strategy;

    public PricingResult CalculatePrice(decimal basePrice, DateTime checkIn, DateTime checkOut)
    {
        return _strategy.Calculate(basePrice, checkIn, checkOut);
    }
}
```

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