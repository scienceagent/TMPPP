using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.ViewModels
{
     public class GuestController : BaseViewModel
     {
          private readonly IUserRepository _userRepository;
          private readonly IUserValidator _userValidator;

          private string _guestName = string.Empty;
          private string _guestEmail = string.Empty;
          private string _guestNationality = string.Empty;
          private string _guestPassport = string.Empty;
          private User? _currentUser;

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

          public User? CurrentUser => _currentUser;

          public event Action<string>? OnLog;

          public GuestController(IUserRepository userRepository, IUserValidator userValidator)
          {
               _userRepository = userRepository;
               _userValidator = userValidator;
          }

          public void CreateGuest()
          {
               try
               {
                    _currentUser = new Guest(
                        Guid.NewGuid().ToString(),
                        GuestName, GuestEmail, "0721234567",
                        GuestNationality, GuestPassport);

                    _userRepository.Save(_currentUser);
                    bool valid = _userValidator.Validate(_currentUser);

                    OnLog?.Invoke("Guest created.");
                    OnLog?.Invoke(_currentUser.GetDisplayInfo());
                    OnLog?.Invoke($"Valid: {valid}\n");

                    if (valid)
                         ToastService.Instance.Show(
                             "Guest Registered",
                             $"{GuestName} added successfully.",
                             ToastKind.Success);
                    else
                         ToastService.Instance.Show(
                             "Validation Warning",
                             "Guest saved but failed validation — check passport and nationality.",
                             ToastKind.Warning);
               }
               catch (Exception ex)
               {
                    OnLog?.Invoke($"Error: {ex.Message}\n");
                    ToastService.Instance.Show("Registration Failed", ex.Message, ToastKind.Error);
               }
          }
     }
}