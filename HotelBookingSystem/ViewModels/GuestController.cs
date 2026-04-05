using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.ViewModels
{
     public class GuestController : BaseViewModel
     {
          private readonly IUserRepository _userRepository;

          private string _guestName = "";
          private string _guestEmail = "";
          private string _guestNationality = "";
          private string _guestPassport = "";
          private string _currentGuestId = "";

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

          public string CurrentGuestId => _currentGuestId;

          public event Action<string>? OnLog;

          public GuestController(IUserRepository userRepository)
          {
               _userRepository = userRepository;
          }

          public void CreateGuest()
          {
               if (string.IsNullOrWhiteSpace(GuestName))
               {
                    ToastService.Instance.Show("Missing Name", "Please enter a guest name.", ToastKind.Warning);
                    return;
               }

               if (string.IsNullOrWhiteSpace(GuestEmail))
               {
                    ToastService.Instance.Show("Missing Email", "Please enter a guest email.", ToastKind.Warning);
                    return;
               }

               var guest = new Guest(
                   Guid.NewGuid().ToString(),
                   GuestName,
                   GuestEmail,
                   "",
                   string.IsNullOrWhiteSpace(GuestNationality) ? "Unknown" : GuestNationality,
                   string.IsNullOrWhiteSpace(GuestPassport) ? "UNKNOWN" : GuestPassport);

               _userRepository.Save(guest);
               _currentGuestId = guest.Id;

               OnLog?.Invoke($"[Guest] Registered: {GuestName} ({GuestEmail})");
               OnLog?.Invoke($"  ID: {guest.Id[..8]}...\n");

               ToastService.Instance.Show(
                   "Guest Registered",
                   $"{GuestName} registered successfully.",
                   ToastKind.Success);
          }
     }
}