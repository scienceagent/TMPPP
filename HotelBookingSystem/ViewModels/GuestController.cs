using System;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models.User;
using HotelBookingSystem.Email; // Add this using directive for GmailEmailService

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

               // Fire-and-forget: Send a welcome email to the newly created user using their provided email address
               _ = SendWelcomeEmailAsync(guest);

               ToastService.Instance.Show(
                   "Guest Registered",
                   $"{GuestName} registered successfully.",
                   ToastKind.Success);
          }

          // Private method to handle the async email delivery without blocking the UI
          private async System.Threading.Tasks.Task SendWelcomeEmailAsync(Guest guest)
          {
               var gmail = new GmailEmailService();
               var emailMessage = new EmailMessage
               {
                    To = guest.Email,
                    Subject = "Welcome to Grand Horizon Hotel PMS!",
                    IsHtml = true,
                    Body = $@"
                        <h3>Welcome, {guest.Name}!</h3>
                        <p>Thank you for registering with Grand Horizon Hotel.</p>
                        <p>Your guest ID is <strong>{guest.Id[..8]}...</strong></p>
                        <br/>
                        <p>We look forward to serving you.</p>"
               };

               var result = await gmail.SendAsync(emailMessage);

               if (result.Success)
               {
                    OnLog?.Invoke($"[Email] ✓ Welcome email sent to {guest.Email}");
               }
               else
               {
                    OnLog?.Invoke($"[Email] ✗ Failed to send welcome email to {guest.Email}: {result.Message}");
               }
          }
     }
}