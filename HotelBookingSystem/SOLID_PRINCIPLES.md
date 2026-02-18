# Laborator 1 – Recapitulare OOP și Principiile SOLID

## Structura aplicației (entități principale)

- **Room** (abstract) – clasă de bază pentru camere  
  - **StandardRoom** – cameră standard  
  - **DeluxeRoom** – cameră deluxe  
  - **Suite** – suită  
- **Booking** – rezervare (asociată la Guest și Room)  
- **Guest** – oaspete  

Interfețe pentru servicii: **IBookingService**, **IPaymentProcessor**, **INotificationService**, **IPriceCalculator**.

---

## OOP – Încapsulare, moștenire, polimorfism

- **Încapsulare**: proprietăți și metode pe fiecare clasă; datele sunt accesate prin API public (get/set, metode).
- **Moștenire**: `StandardRoom`, `DeluxeRoom`, `Suite` moștenesc `Room`; folosesc constructori protejați și suprascriere (ex: `GetRoomDetails()`, `Type`).
- **Polimorfism**: oriunde se folosește `Room` se poate folosi orice subclasă; `GetRoomDetails()` și `Type` sunt polimorfice.

---

## Aplicarea principiilor SOLID

### SRP (Single Responsibility Principle)

- **Room** – reprezintă o cameră (date + comportament de bază).
- **Booking** – reprezintă o rezervare.
- **Guest** – reprezintă un oaspete.
- **BookingService** – coordonează crearea/anularea rezervărilor (o singură responsabilitate).
- **PaymentProcessor** – procesează plăți.
- **EmailNotificationService** – trimite notificări.
- **DiscountPriceCalculator** / **StandardPriceCalculator** – calculează prețul (o singură responsabilitate).
- ViewModelele au o singură responsabilitate: prezentare și logică UI pentru o secțiune.

### OCP (Open/Closed Principle)

- Noi tipuri de camere se adaugă prin subclase ale lui `Room`, fără a modifica `Room` sau restul codului care folosește `Room`.
- Noi strategii de preț: implementări noi ale `IPriceCalculator`, fără a modifica `BookingService`.
- Noi moduri de plată/notificare: implementări noi ale `IPaymentProcessor` / `INotificationService`.

### LSP (Liskov Substitution Principle)

- Orice `StandardRoom`, `DeluxeRoom` sau `Suite` poate fi folosit acolo unde se așteaptă `Room`, fără a strica comportamentul (ex: listă de camere, calcul preț, afișare detalii).

### ISP (Interface Segregation Principle)

- **IBookingService** – doar operații de booking (CreateBooking, CancelBooking).
- **IPaymentProcessor** – doar ProcessPayment.
- **INotificationService** – doar SendNotification.
- **IPriceCalculator** – doar CalculatePrice.  
Nicio clasă nu e forțată să implementeze metode inutile.

### DIP (Dependency Inversion Principle)

- **BookingService** depinde de `IPaymentProcessor`, `INotificationService`, `IPriceCalculator` (abstracții), nu de clase concrete.
- **MainViewModel** primește/composează `IBookingService`; nu creează direct implementări concrete ale plății/notificărilor/calculului de preț – dependențele sunt “injectate” prin crearea serviciilor în constructor (injecție manuală de dependențe).

---

## Refactorizare

- Clasele de domeniu (Room, Booking, Guest) sunt separate de servicii și UI.
- Serviciile depind doar de interfețe.
- Interfețele sunt mici și specifice (ISP).
- Extensibilitatea se face prin clase noi și implementări noi de interfețe, fără modificări majore (OCP).

---

## Prezentare rezultat

La prezentare se poate evidenția:

1. **Structura**: diagramă UML simplă – Room (abstract), StandardRoom, DeluxeRoom, Suite, Booking, Guest și interfețele IBookingService, IPaymentProcessor, INotificationService, IPriceCalculator.
2. **OOP**: încapsulare (proprietăți, metode), moștenire (Room → StandardRoom/DeluxeRoom/Suite), polimorfism (GetRoomDetails(), Type).
3. **SOLID**: fiecare principiu aplicat conform secțiunii de mai sus (SRP, OCP, LSP, ISP, DIP).
4. **Modularitate**: domeniu vs. servicii vs. UI; dependențe prin interfețe și injecție (manuală) de dependențe.
