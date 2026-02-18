# Structura Proiect WPF Progresiv - Hotel Booking

## Structura Lab 1 (implementată)

```
HotelBookingSystem/
│
├── Models/                          # Entități de domeniu
│   ├── Enums.cs                     ✅ LAB 1
│   ├── User.cs                      ✅ LAB 1
│   ├── Guest.cs                     ✅ LAB 1
│   ├── Admin.cs                     ✅ LAB 1
│   ├── Room.cs                      ✅ LAB 1
│   ├── StandardRoom.cs              ✅ LAB 1
│   ├── DeluxeRoom.cs                ✅ LAB 1
│   └── Booking.cs                   ✅ LAB 1
│
├── Interfaces/                      # Abstracții (ISP)
│   ├── IBookingService.cs           ✅ LAB 1
│   ├── IPaymentProcessor.cs         ✅ LAB 1
│   └── IPriceCalculator.cs          ✅ LAB 1
│
├── Services/                        # Logică business (DIP)
│   ├── BookingService.cs            ✅ LAB 1
│   ├── SimplePaymentProcessor.cs    ✅ LAB 1
│   └── StandardPriceCalculator.cs   ✅ LAB 1
│
├── ViewModels/                      # MVVM - ViewModels
│   ├── BaseViewModel.cs              ✅ LAB 1 (infrastructură)
│   └── MainViewModel.cs             ✅ LAB 1 (minimal)
│
├── Views/                           # MVVM - Views (gol pentru Lab 1)
│   └── (MainWindow la rădăcină)
│
├── Commands/                        # MVVM Infrastructure
│   └── RelayCommand.cs              ✅ LAB 1
│
├── Data/                            # Database (viitor)
│   └── README.txt                   ⏳ LAB 3
│
├── Helpers/                         # Utilities (viitor)
│   └── README.txt                   ⏳
│
├── MainWindow.xaml                  ✅ LAB 1 (UI primitiv)
├── MainWindow.xaml.cs               ✅ LAB 1
├── App.xaml                         ✅ LAB 1
└── App.xaml.cs                      ✅ LAB 1
```

## Ce este implementat în LAB 1

- **Models/** – 8 fișiere (Enums, User, Guest, Admin, Room, StandardRoom, DeluxeRoom, Booking)
- **Interfaces/** – 3 fișiere (IBookingService, IPaymentProcessor, IPriceCalculator)
- **Services/** – 3 fișiere (BookingService, SimplePaymentProcessor, StandardPriceCalculator)
- **ViewModels/** – 2 fișiere (BaseViewModel, MainViewModel)
- **Commands/** – RelayCommand.cs
- **MainWindow** – UI primitiv: Test Creare Guest, Test Creare Room, Test Booking, Log Output

## Evoluție progresivă

- **Lab 2**: RoomListViewModel, BookingViewModel, Factory, Strategy, Observer
- **Lab 3**: Data/AppDbContext, Repositories, IRepository
- **Lab 4**: UI complet, AdminViewModel, multiple Views
