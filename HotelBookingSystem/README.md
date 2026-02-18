# 🏨 Hotel Booking System — TMPPP Laboratory Work

A WPF desktop application built with C# demonstrating Object-Oriented Programming principles, SOLID design, and Creational Design Patterns through a real-world Hotel Booking System.

**Student:** Grigoriev  
**Course:** TMPPP (Software Design Techniques and Programming Paradigms)  
**Technology:** C# / WPF / .NET

---

## 📁 Project Structure

```
HotelBookingSystem/
├── Models/                     # Domain entities (User, Room, Booking)
├── Interfaces/                 # Abstraction layer (SOLID + Patterns)
├── Services/                   # Business logic implementations
├── Factories/                  # Creational design patterns (Lab 2)
│   ├── Pricing/                # Pricing strategy products
│   └── Confirmation/           # Confirmation handler products
├── ViewModels/                 # Controllers + MainViewModel (MVVM)
├── Commands/                   # WPF command infrastructure
├── Converters/                 # XAML value converters
├── MainWindow.xaml             # UI layout
└── App.xaml                    # Application entry point
```

---

## 🔬 Laboratory 1 — OOP Recap and SOLID Principles

### Objective

Define and implement a set of classes using the fundamental concepts of OOP: **encapsulation**, **inheritance**, and **polymorphism**. Each class must adhere to the **SOLID** principles to ensure code clarity, flexibility, and ease of maintenance.

### OOP Concepts Demonstrated

| Concept | Where it's applied |
|---|---|
| **Encapsulation** | All models use `private set` / `protected set` with public getters. Constructors enforce valid state. |
| **Inheritance** | `User` → `Guest`, `Admin`. `Room` → `StandardRoom`, `DeluxeRoom`, `Suite`. `BaseViewModel` → all controllers. |
| **Polymorphism (Dynamic)** | `virtual` + `override` on `GetDisplayInfo()`, `GetDescription()`, `SetAvailability()` — runtime dispatch. |
| **Abstraction** | `abstract class User`, `abstract class Room`, interfaces like `IBookingService`, `IRoomRepository`. |

### SOLID Principles

#### S — Single Responsibility Principle

Each controller handles ONE concern:

| Controller | Responsibility |
|---|---|
| `GuestController` | Guest registration and validation |
| `RoomController` | Room creation and pricing display |
| `BookingController` | Booking lifecycle (create, confirm, cancel) |
| `LogController` | Activity logging |

#### O — Open/Closed Principle

New room types can be added by creating a new class (e.g., `Penthouse : Room`) without modifying existing code. The inheritance hierarchy is open for extension but closed for modification.

#### L — Liskov Substitution Principle

Every virtual/abstract method in base classes is overridden in ALL child classes:

| Base Method | StandardRoom | DeluxeRoom | Suite |
|---|---|---|---|
| `Capacity` (abstract) | ✅ override | ✅ override | ✅ override |
| `GetDisplayInfo()` | ✅ override | ✅ override | ✅ override |
| `GetDescription()` | ✅ override | ✅ override | ✅ override |
| `SetAvailability()` | ✅ override | ✅ override | ✅ override |

Any child can fully replace its parent without breaking behavior.

#### I — Interface Segregation Principle

Small, focused interfaces — no client depends on methods it doesn't use:

| Interface | Methods | Used by |
|---|---|---|
| `IUserValidator` | `Validate()` | GuestController |
| `ILogger` | `Info()`, `Warn()`, `Error()` | LogController (uses only `Info()`) |
| `IRoomPricingService` | `CalculatePrice()`, `CalculateCleaningCost()` | RoomController |
| `IBookingDurationCalculator` | `CalculateDuration()`, `CalculateNights()`, `IsLongStay()` | BookingController (uses only `CalculateNights()`, `IsLongStay()`) |

Controllers do NOT use 100% of interface methods — demonstrating ISP compliance.

#### D — Dependency Inversion Principle

All controllers depend on **abstractions (interfaces)**, never on concrete classes:

```
Controller  →  Interface  ←  Service

GuestController  →  IUserRepository    ←  InMemoryUserRepository
GuestController  →  IUserValidator     ←  UserValidator
RoomController   →  IRoomRepository    ←  InMemoryRoomRepository
BookingController →  IBookingService   ←  BookingService
LogController    →  ILogger            ←  ConsoleLogger
```

Concrete implementations are created ONLY in `MainViewModel` (Composition Root).

### Class Diagram

UML diagram: `HotelBookingSystem/ClassDiagram1.cd`

---

## 🔬 Laboratory 2 — Creational Design Patterns

### Objective

Implement two creational design patterns: **Factory Method** and **Abstract Factory** to manage object creation in a flexible way, reducing dependencies and improving scalability.

### Pattern 1: Factory Method — Room Creation

**Problem:** `RoomController` used a `switch` statement with `new StandardRoom()`, `new DeluxeRoom()`, `new Suite()` — tightly coupled to concrete classes.

**Solution:** `IRoomFactory` interface with concrete factories for each room type.

#### Structure

```
RoomController
     │
     ▼
RoomFactoryProvider
     │  GetFactory("Deluxe")
     ▼
IRoomFactory (interface)
     │  CreateRoom(id, number, price, capacity) : Room
     │
     ├── StandardRoomFactory  →  StandardRoom
     ├── DeluxeRoomFactory    →  DeluxeRoom
     └── SuiteRoomFactory     →  Suite
```

#### Files

| File | Role |
|---|---|
| `Interfaces/IRoomFactory.cs` | Factory method interface |
| `Factories/StandardRoomFactory.cs` | Creates `StandardRoom` |
| `Factories/DeluxeRoomFactory.cs` | Creates `DeluxeRoom` |
| `Factories/SuiteRoomFactory.cs` | Creates `Suite` |
| `Factories/RoomFactoryProvider.cs` | Selects correct factory by room type string |

#### Before (Lab 1) vs After (Lab 2)

```csharp
// BEFORE — switch + direct instantiation
switch (SelectedRoomType)
{
    case "Standard":
        _currentRoom = new StandardRoom(id, number, price, capacity);
        break;
    case "Deluxe":
        _currentRoom = new DeluxeRoom(id, number, price, capacity, amenities, true);
        break;
    case "Suite":
        _currentRoom = new Suite(id, number, price, capacity, true, true);
        break;
}

// AFTER — Factory Method
IRoomFactory factory = _roomFactoryProvider.GetFactory(SelectedRoomType);
_currentRoom = factory.CreateRoom(id, number, price, capacity);
```

### Pattern 2: Abstract Factory — Booking Families

**Problem:** Different booking types (Standard, Premium, VIP) need different pricing logic AND different confirmation messages. These objects must be created together as a family.

**Solution:** `IBookingFactory` interface that creates a **family** of related objects: `Booking` + `IPricingStrategy` + `IConfirmationHandler`.

#### Structure

```
BookingController
     │
     ▼
BookingFactoryProvider
     │  GetFactory("VIP")
     ▼
IBookingFactory (interface)
     │  CreateBooking()             → Booking
     │  CreatePricingStrategy()     → IPricingStrategy
     │  CreateConfirmationHandler() → IConfirmationHandler
     │
     ├── StandardBookingFactory
     │     ├── Booking
     │     ├── StandardPricingStrategy     (base rate)
     │     └── StandardConfirmationHandler
     │
     ├── PremiumBookingFactory
     │     ├── Booking
     │     ├── PremiumPricingStrategy      (10% discount)
     │     └── PremiumConfirmationHandler
     │
     └── VipBookingFactory
           ├── Booking
           ├── VipPricingStrategy          (20% + free nights)
           └── VipConfirmationHandler
```

#### Files

| File | Role |
|---|---|
| `Interfaces/IBookingFactory.cs` | Abstract factory interface |
| `Interfaces/IPricingStrategy.cs` | Pricing product interface |
| `Interfaces/IConfirmationHandler.cs` | Confirmation product interface |
| `Factories/StandardBookingFactory.cs` | Creates standard family |
| `Factories/PremiumBookingFactory.cs` | Creates premium family |
| `Factories/VipBookingFactory.cs` | Creates VIP family |
| `Factories/Pricing/StandardPricingStrategy.cs` | Base rate × nights |
| `Factories/Pricing/PremiumPricingStrategy.cs` | 10% discount |
| `Factories/Pricing/VipPricingStrategy.cs` | 20% discount + free nights |
| `Factories/Confirmation/StandardConfirmationHandler.cs` | Basic confirmation |
| `Factories/Confirmation/PremiumConfirmationHandler.cs` | + Early check-in perks |
| `Factories/Confirmation/VipConfirmationHandler.cs` | + Spa, airport transfer |
| `Factories/BookingFactoryProvider.cs` | Selects correct factory by type |

#### Booking Types Comparison

| Type | Pricing | Confirmation | Benefits |
|---|---|---|---|
| **Standard** | Base rate × nights | Basic confirmation | — |
| **Premium** | 10% discount | Enhanced confirmation | Early check-in, late check-out |
| **VIP** | 20% discount + 1 free night every 5 | Premium confirmation | Spa, airport transfer, free minibar |

### Key Difference Between Patterns

| | Factory Method | Abstract Factory |
|---|---|---|
| **Creates** | ONE product | A FAMILY of related products |
| **In this project** | `IRoomFactory` → 1 Room | `IBookingFactory` → Booking + Pricing + Confirmation |
| **Extensibility** | Add new room type = 1 new factory | Add new booking tier = 1 factory + 1 pricing + 1 confirmation |
| **Client** | `RoomController` | `BookingController` |

### UML Diagrams

- Factory Method: `HotelBookingSystem/Lab2_FactoryMethod.puml`
- Abstract Factory: `HotelBookingSystem/Lab2_AbstractFactory.puml`

---

## 🚀 How to Run

1. Open `HotelBookingSystem.sln` in Visual Studio 2022
2. Set `HotelBookingSystem` as the startup project
3. Press `F5` to build and run
4. Use the sidebar to navigate:
   - **+ New Booking** — Register guest → Assign room → Select booking type → Create booking
   - **Bookings** — View, confirm, or cancel bookings
   - **Activity Log** — See all system events

---

## 🏗️ Architecture Overview

```
┌──────────────────────────────────────────────────┐
│                   MainWindow.xaml                 │
│                  (WPF UI Layer)                   │
└──────────────────────┬───────────────────────────┘
                       │ binds to
┌──────────────────────▼───────────────────────────┐
│                 MainViewModel                     │
│              (Composition Root)                   │
│  Creates all services, factories, controllers     │
└──┬──────────┬──────────┬──────────┬──────────────┘
   │          │          │          │
   ▼          ▼          ▼          ▼
 Guest     Room      Booking     Log
 Ctrl      Ctrl       Ctrl      Ctrl
   │          │          │          │
   ▼          ▼          ▼          ▼
Interfaces Interfaces Interfaces Interfaces
   │          │          │          │
   ▼          ▼          ▼          ▼
Services   Factories  Factories  Services
           (Lab 2)    (Lab 2)
```