using System;
using System.Collections.Generic;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     public class RoomController : BaseViewModel
     {
          private readonly IRoomRepository _roomRepository;
          private readonly IRoomPricingService _roomPricingService;

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
               set => SetProperty(ref _selectedRoomType, value);
          }

          public List<string> RoomTypes { get; } = new List<string> { "Standard", "Deluxe", "Suite" };

          public Room CurrentRoom => _currentRoom;

          public event Action<string> OnLog;

          public RoomController(IRoomRepository roomRepository, IRoomPricingService roomPricingService)
          {
               _roomRepository = roomRepository;
               _roomPricingService = roomPricingService;

               RoomNumber = "101";
               RoomPrice = 200;
               RoomCapacity = 2;
               SelectedRoomType = "Standard";
          }

          public void CreateRoom()
          {
               try
               {
                    switch (SelectedRoomType)
                    {
                         case "Standard":
                              _currentRoom = new StandardRoom(
                                  Guid.NewGuid().ToString(),
                                  RoomNumber,
                                  RoomPrice,
                                  RoomCapacity
                              );
                              break;

                         case "Deluxe":
                              _currentRoom = new DeluxeRoom(
                                  Guid.NewGuid().ToString(),
                                  RoomNumber,
                                  RoomPrice,
                                  RoomCapacity,
                                  new List<string> { "Minibar", "Balcony", "Sea View" },
                                  true
                              );
                              break;

                         case "Suite":
                              _currentRoom = new Suite(
                                  Guid.NewGuid().ToString(),
                                  RoomNumber,
                                  RoomPrice,
                                  3,
                                  true,
                                  true
                              );
                              break;
                    }
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

          public decimal GetPrice(Room room) => _roomPricingService.CalculatePrice(room);

          public void TestPolymorphism()
          {
               Room standard = new StandardRoom("R1", "101", 200m, 2);
               Room deluxe = new DeluxeRoom("R2", "201", 350m, 2, new List<string> { "Minibar" }, true);
               Room suite = new Suite("R3", "301", 500m, 3, true, true);

               OnLog?.Invoke("Standard room:");
               OnLog?.Invoke(standard.GetDisplayInfo());
               OnLog?.Invoke(standard.GetDescription());
               OnLog?.Invoke($"Price: {FormatUsd(_roomPricingService.CalculatePrice(standard))}");
               OnLog?.Invoke($"Cleaning: {FormatUsd(_roomPricingService.CalculateCleaningCost(standard))}\n");

               OnLog?.Invoke("Deluxe room:");
               OnLog?.Invoke(deluxe.GetDisplayInfo());
               OnLog?.Invoke(deluxe.GetDescription());
               OnLog?.Invoke($"Price: {FormatUsd(_roomPricingService.CalculatePrice(deluxe))}");
               OnLog?.Invoke($"Cleaning: {FormatUsd(_roomPricingService.CalculateCleaningCost(deluxe))}\n");

               OnLog?.Invoke("Suite:");
               OnLog?.Invoke(suite.GetDisplayInfo());
               OnLog?.Invoke(suite.GetDescription());
               OnLog?.Invoke($"Price: {FormatUsd(_roomPricingService.CalculatePrice(suite))}");
               OnLog?.Invoke($"Cleaning: {FormatUsd(_roomPricingService.CalculateCleaningCost(suite))}\n");
          }

          private static string FormatUsd(decimal amount) =>
              amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
     }
}