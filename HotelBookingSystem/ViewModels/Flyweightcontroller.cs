using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HotelBookingSystem.Flyweight;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.ViewModels
{
     public class FlyweightController : BaseViewModel
     {
          private readonly RoomAmenityFactory _factory;
          private readonly RoomAmenityRenderer _renderer;
          private readonly IRoomRepository _roomRepository;

          private string _flyweightReport = "Click 'Load Room Amenities' to demonstrate the Flyweight pattern.";
          private int _totalEntries;
          private int _flyweightObjectCount;
          private string _selectedAmenity = "WiFi";

          public ObservableCollection<string> AmenityTypes { get; } = new();
          public ObservableCollection<FlyweightEntryViewModel> Entries { get; } = new();
          public ObservableCollection<FlyweightCacheEntry> CacheEntries { get; } = new();

          public string FlyweightReport
          {
               get => _flyweightReport;
               set => SetProperty(ref _flyweightReport, value);
          }

          public int TotalEntries
          {
               get => _totalEntries;
               set => SetProperty(ref _totalEntries, value);
          }

          public int FlyweightObjectCount
          {
               get => _flyweightObjectCount;
               set => SetProperty(ref _flyweightObjectCount, value);
          }

          public string SelectedAmenity
          {
               get => _selectedAmenity;
               set => SetProperty(ref _selectedAmenity, value);
          }

          public event Action<string>? OnLog;

          public FlyweightController(IRoomRepository roomRepository)
          {
               _roomRepository = roomRepository;
               _factory = new RoomAmenityFactory();
               _renderer = new RoomAmenityRenderer(_factory);

               foreach (var t in _factory.GetAvailableAmenityTypes())
                    AmenityTypes.Add(t);
          }

          public void LoadRoomAmenities()
          {
               Entries.Clear();
               CacheEntries.Clear();

               var rooms = _roomRepository.GetAllRooms();

               if (rooms.Count == 0)
               {
                    FlyweightReport = "No rooms found. Create some rooms first on the New Booking page.";
                    OnLog?.Invoke("[Flyweight] No rooms in repository — create rooms first.\n");
                    return;
               }

               // Simulate many amenity entries — all rooms get standard amenities
               // plus type-specific ones. The key: only one flyweight per amenity TYPE.
               var standardAmenities = new[] { "WiFi", "Air Conditioning", "Room Service" };
               var deluxeAmenities = new[] { "WiFi", "Minibar", "Balcony", "Sea View", "Concierge" };
               var suiteAmenities = new[] { "WiFi", "Minibar", "Spa", "Kitchen", "Concierge", "Airport Shuttle" };

               foreach (var room in rooms)
               {
                    // Pick amenity set based on room type name
                    string[] amenities;
                    if (room.GetType().Name.Contains("Suite"))
                         amenities = suiteAmenities;
                    else if (room.GetType().Name.Contains("Deluxe"))
                         amenities = deluxeAmenities;
                    else
                         amenities = standardAmenities;

                    foreach (var a in amenities)
                    {
                         _renderer.AddRoomAmenity(room.RoomId, room.RoomNumber, room.BasePrice, a);
                         Entries.Add(new FlyweightEntryViewModel
                         {
                              RoomNumber = room.RoomNumber,
                              AmenityType = a,
                              Icon = _factory.GetOrCreate(a).Icon,
                              Color = _factory.GetOrCreate(a).Color,
                              Category = _factory.GetOrCreate(a).Category,
                              IsShared = true
                         });
                    }
               }

               // Populate cache view
               foreach (var kv in _factory.GetCache())
               {
                    CacheEntries.Add(new FlyweightCacheEntry
                    {
                         Key = kv.Key,
                         Icon = kv.Value.Icon,
                         Color = kv.Value.Color,
                         Category = kv.Value.Category
                    });
               }

               TotalEntries = Entries.Count;
               FlyweightObjectCount = _factory.CacheSize;

               FlyweightReport =
                   $"✓ Loaded {TotalEntries} amenity entries across {rooms.Count} rooms.\n" +
                   $"  Flyweight objects created: {FlyweightObjectCount} (one per unique amenity type).\n" +
                   $"  Without Flyweight: {TotalEntries} separate objects.\n" +
                   $"  With Flyweight:    {FlyweightObjectCount} shared objects + {TotalEntries} lightweight entry refs.\n" +
                   $"  Memory reduction:  ~{TotalEntries - FlyweightObjectCount} redundant objects eliminated.";

               OnLog?.Invoke($"[Flyweight] Loaded {TotalEntries} entries sharing {FlyweightObjectCount} flyweight objects.");
               OnLog?.Invoke($"  Without pattern: {TotalEntries} full amenity objects in memory.");
               OnLog?.Invoke($"  With Flyweight:  {FlyweightObjectCount} objects (intrinsic state shared).\n");
          }
     }

     public class FlyweightEntryViewModel
     {
          public string RoomNumber { get; set; } = "";
          public string AmenityType { get; set; } = "";
          public string Icon { get; set; } = "";
          public string Color { get; set; } = "#757575";
          public string Category { get; set; } = "";
          public bool IsShared { get; set; }

          public string IconAndType => $"{Icon}  {AmenityType}";
     }

     public class FlyweightCacheEntry
     {
          public string Key { get; set; } = "";
          public string Icon { get; set; } = "";
          public string Color { get; set; } = "#757575";
          public string Category { get; set; } = "";

          public string Display => $"{Icon}  {Key}";
     }
}