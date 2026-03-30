using System;
using System.Collections.Generic;
using HotelBookingSystem.Factories;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using HotelBookingSystem.Prototype;

namespace HotelBookingSystem.ViewModels
{
     public class RoomController : BaseViewModel
     {
          private readonly IRoomRepository _roomRepository;
          private readonly IRoomPricingService _roomPricingService;
          private readonly RoomCreatorProvider _creatorProvider;
          private readonly RoomPrototypeRegistry _registry;
          private RoomCreator? _creator;
          private Room? _currentRoom;

          private string? _roomNumber;
          private decimal _roomPrice;
          private int _roomCapacity;
          private string? _selectedRoomType;

          public string? RoomNumber { get => _roomNumber; set => SetProperty(ref _roomNumber, value); }
          public decimal RoomPrice { get => _roomPrice; set => SetProperty(ref _roomPrice, value); }
          public int RoomCapacity { get => _roomCapacity; set => SetProperty(ref _roomCapacity, value); }

          public string? SelectedRoomType
          {
               get => _selectedRoomType;
               set
               {
                    if (SetProperty(ref _selectedRoomType, value))
                    {
                         _creator = _creatorProvider.GetCreator(value ?? "Standard");
                         OnPropertyChanged(nameof(CapacityLabel));
                         var snapshot = _registry.GetClone(value ?? "Standard");
                         RoomPrice = snapshot.BasePrice;
                         RoomCapacity = snapshot.Capacity;
                    }
               }
          }

          public string CapacityLabel => SelectedRoomType == "Suite" ? "Number of Rooms" : "Capacity (Guests)";
          public List<string> RoomTypes { get; }
          public Room? CurrentRoom => _currentRoom;

          public event Action<string>? OnLog;

          public RoomController(IRoomRepository roomRepository,
                                IRoomPricingService roomPricingService,
                                RoomPrototypeRegistry registry)
          {
               _roomRepository = roomRepository;
               _roomPricingService = roomPricingService;
               _registry = registry;
               _creatorProvider = new RoomCreatorProvider();
               RoomTypes = new List<string>(_creatorProvider.GetAvailableTypes());
               RoomNumber = "101";
               SelectedRoomType = "Standard";
          }

          public void CreateRoom()
          {
               try
               {
                    var snapshot = _registry.GetClone(SelectedRoomType ?? "Standard");
                    snapshot.RoomNumber = RoomNumber ?? "";
                    snapshot.BasePrice = RoomPrice;
                    snapshot.Capacity = RoomCapacity;

                    OnLog?.Invoke($"[Prototype] Cloned '{SelectedRoomType}' template -> Room {RoomNumber}");
                    OnLog?.Invoke($"  {snapshot.DisplayInfo}");

                    var (success, error, room, display, description, priceSummary, cleaningCost) =
                        _creator!.CreateRoom(RoomNumber ?? "", RoomPrice, RoomCapacity,
                                             _roomRepository, _roomPricingService);

                    if (!success)
                    {
                         OnLog?.Invoke($"Room creation failed: {error}\n");
                         ToastService.Instance.Show("Room Not Created", error ?? "Unknown error.", ToastKind.Error);
                         return;
                    }

                    _currentRoom = room;
                    OnLog?.Invoke($"[Factory Method] {SelectedRoomType} room created.");
                    OnLog?.Invoke(display!);
                    OnLog?.Invoke(description!);
                    OnLog?.Invoke(priceSummary!);
                    OnLog?.Invoke($"Cleaning cost: {FormatUsd(cleaningCost)}\n");

                    ToastService.Instance.Show(
                        "Room Assigned",
                        $"{SelectedRoomType} room {RoomNumber} ready at {Usd(RoomPrice)}/night.",
                        ToastKind.Success);
               }
               catch (Exception ex)
               {
                    OnLog?.Invoke($"Error: {ex.Message}\n");
                    ToastService.Instance.Show("Room Error", ex.Message, ToastKind.Error);
               }
          }

          private static string FormatUsd(decimal v) =>
              v.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

          private static string Usd(decimal v) =>
              v.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
     }
}