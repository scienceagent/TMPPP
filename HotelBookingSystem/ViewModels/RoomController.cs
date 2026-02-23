using System;
using System.Collections.Generic;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     public class RoomController : BaseViewModel
     {
          private readonly IRoomRepository _roomRepository;
          private readonly IRoomPricingService _roomPricingService;
          private readonly RoomCreatorProvider _creatorProvider;
          private RoomCreator _creator;
          private Room _currentRoom;

          private string _roomNumber;
          private decimal _roomPrice;
          private int _roomCapacity;
          private string _selectedRoomType;

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
                         _creator = _creatorProvider.GetCreator(value ?? "Standard");
                         OnPropertyChanged(nameof(CapacityLabel));
                    }
               }
          }

          public string CapacityLabel =>
              SelectedRoomType == "Suite" ? "Number of Rooms" : "Capacity (Guests)";

          public List<string> RoomTypes { get; }
          public Room CurrentRoom => _currentRoom;

          public event Action<string> OnLog;

          public RoomController(IRoomRepository roomRepository, IRoomPricingService roomPricingService)
          {
               _roomRepository = roomRepository;
               _roomPricingService = roomPricingService;
               _creatorProvider = new RoomCreatorProvider();

               RoomTypes = new List<string>(_creatorProvider.GetAvailableTypes());

               RoomNumber = "101";
               RoomPrice = 200m;
               RoomCapacity = 2;
               SelectedRoomType = "Standard";
          }

          public void CreateRoom()
          {
               try
               {
                    var (success, error, room, display, description, priceSummary, cleaningCost) =
                        _creator.CreateRoom(RoomNumber, RoomPrice, RoomCapacity, _roomRepository, _roomPricingService);

                    if (!success)
                    {
                         OnLog?.Invoke($"Room creation failed: {error}\n");
                         return;
                    }

                    _currentRoom = room;

                    OnLog?.Invoke($"{SelectedRoomType} room created.");
                    OnLog?.Invoke(display);
                    OnLog?.Invoke(description);
                    OnLog?.Invoke(priceSummary);
                    OnLog?.Invoke($"Cleaning cost: {FormatUsd(cleaningCost)}");
                    OnLog?.Invoke($"Capacity: {room.Capacity} guests\n");
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