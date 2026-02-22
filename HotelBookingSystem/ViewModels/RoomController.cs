using System;
using System.Collections.Generic;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     public class RoomController : BaseViewModel
     {
          private readonly IRoomRepository _roomRepository;
          private readonly IRoomPricingService _roomPricingService;
          private readonly RoomFactoryProvider _roomFactoryProvider;

          private string _roomNumber;
          private decimal _roomPrice;
          private int _roomCapacity;
          private string _selectedRoomType;
          private Room _currentRoom;

          public string RoomNumber
          {
               get => _roomNumber;
               set => SetProperty(ref _roomNumber, value);
          }

          public decimal RoomPrice
          {
               get => _roomPrice;
               set => SetProperty(ref _roomPrice, value);
          }

          public int RoomCapacity
          {
               get => _roomCapacity;
               set => SetProperty(ref _roomCapacity, value);
          }

          public string SelectedRoomType
          {
               get => _selectedRoomType;
               set
               {
                    if (SetProperty(ref _selectedRoomType, value))
                    {
                         // Notify XAML that the capacity label also changed when room type changes
                         OnPropertyChanged(nameof(CapacityLabel));
                    }
               }
          }

          // Dynamic label used in Step 2:
          // Suite expects "number of rooms" input (capacity = rooms x 2)
          // All other types expect "number of guests" input directly
          public string CapacityLabel =>
              SelectedRoomType == "Suite" ? "Number of Rooms" : "Capacity (Guests)";

          public List<string> RoomTypes { get; } = new List<string> { "Standard", "Deluxe", "Suite" };

          public Room CurrentRoom => _currentRoom;

          public event Action<string> OnLog;

          public RoomController(IRoomRepository roomRepository, IRoomPricingService roomPricingService)
          {
               _roomRepository = roomRepository;
               _roomPricingService = roomPricingService;
               _roomFactoryProvider = new RoomFactoryProvider();

               RoomNumber = "101";
               RoomPrice = 200;
               RoomCapacity = 2;
               SelectedRoomType = "Standard";
          }

          public void CreateRoom()
          {
               try
               {
                    // FACTORY METHOD — decouples RoomController from concrete room classes
                    IRoomFactory factory = _roomFactoryProvider.GetFactory(SelectedRoomType);
                    _currentRoom = factory.CreateRoom(
                        Guid.NewGuid().ToString(),
                        RoomNumber,
                        RoomPrice,
                        RoomCapacity
                    );

                    _roomRepository.Save(_currentRoom);

                    OnLog?.Invoke($"{SelectedRoomType} room created.");
                    OnLog?.Invoke(_currentRoom.GetDisplayInfo());
                    OnLog?.Invoke(_currentRoom.GetDescription());
                    OnLog?.Invoke($"Price: {FormatUsd(_roomPricingService.CalculatePrice(_currentRoom))}");
                    OnLog?.Invoke($"Cleaning cost: {FormatUsd(_roomPricingService.CalculateCleaningCost(_currentRoom))}");
                    OnLog?.Invoke($"Capacity: {_currentRoom.Capacity} guests\n");
               }
               catch (Exception ex)
               {
                    OnLog?.Invoke($"Error: {ex.Message}\n");
               }
          }

          private static string FormatUsd(decimal amount) =>
              amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
     }
}