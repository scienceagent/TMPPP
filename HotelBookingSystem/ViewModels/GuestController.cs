using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.ViewModels
{
     public class GuestController : BaseViewModel
     {
          private readonly IUserRepository _userRepository;
          private readonly IUserValidator _userValidator;

          private string _guestName;
          private string _guestEmail;
          private string _guestNationality;
          private string _guestPassport;
          private User _currentUser;

          public string GuestName
          {
               get => _guestName;
               set => SetProperty(ref _guestName, value);
          }

          public string GuestEmail
          {
               get => _guestEmail;
               set => SetProperty(ref _guestEmail, value);
          }

          public string GuestNationality
          {
               get => _guestNationality;
               set => SetProperty(ref _guestNationality, value);
          }

          public string GuestPassport
          {
               get => _guestPassport;
               set => SetProperty(ref _guestPassport, value);
          }

          public User CurrentUser => _currentUser;

          // Event to notify MainViewModel when something happens
          public event Action<string> OnLog;

          public GuestController(IUserRepository userRepository, IUserValidator userValidator)
          {
               _userRepository = userRepository;
               _userValidator = userValidator;

               GuestName = "";
               GuestEmail = "";
               GuestNationality = "";
               GuestPassport = "";
          }

          public void CreateGuest()
          {
               try
               {
                    _currentUser = new Guest(
                        Guid.NewGuid().ToString(),
                        GuestName,
                        GuestEmail,
                        "0721234567",
                        GuestNationality,
                        GuestPassport
                    );
                    _userRepository.Save(_currentUser);

                    OnLog?.Invoke("Guest created.");
                    OnLog?.Invoke(_currentUser.GetDisplayInfo());
                    OnLog?.Invoke($"Valid: {_userValidator.Validate(_currentUser)}\n");
               }
               catch (Exception ex)
               {
                    OnLog?.Invoke($"Error: {ex.Message}\n");
               }
          }

          public bool ValidateCurrentUser()
          {
               return _currentUser != null && _userValidator.Validate(_currentUser);
          }

          public void TestPolymorphism()
          {
               User guest = new Guest("G1", "Test Guest", "guest@test.com", "0721111111", "US", "US111");
               User admin = new Admin("A1", "Test Admin", "admin@test.com", "0721222222",
                                      "Manager", "IT",
                                      new System.Collections.Generic.List<string> { "ManageRooms", "ViewBookings", "ManageUsers" });

               OnLog?.Invoke("Guest info:");
               OnLog?.Invoke(guest.GetDisplayInfo());
               OnLog?.Invoke($"Valid: {_userValidator.Validate(guest)}\n");

               OnLog?.Invoke("Admin info:");
               OnLog?.Invoke(admin.GetDisplayInfo());
               OnLog?.Invoke($"Valid: {_userValidator.Validate(admin)}\n");
          }
     }
}