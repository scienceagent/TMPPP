using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.ViewModels
{
     public class LogController : BaseViewModel
     {
          private readonly ILogger _logger;
          private string _logOutput;

          public string LogOutput
          {
               get => _logOutput;
               set => SetProperty(ref _logOutput, value);
          }

          public LogController(ILogger logger)
          {
               _logger = logger;
               LogOutput = "Hotel Booking System ready.\n" +
                          "Create a guest, then a room, then complete your booking.\n\n";
          }

          public void AddLog(string message)
          {
               LogOutput += $"{message}\n";
               _logger.Info(message);
          }
     }
}