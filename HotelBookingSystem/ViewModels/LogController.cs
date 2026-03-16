using System;
using System.Linq;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.ViewModels
{
     public class LogController : BaseViewModel
     {
          private readonly ILogger _logger;
          private string _logOutput;
          private const int MaxLines = 300;

          public string LogOutput
          {
               get => _logOutput;
               set => SetProperty(ref _logOutput, value);
          }

          public LogController(ILogger logger)
          {
               _logger = logger;
               _logOutput =
                   "Hotel Booking System ready.\n" +
                   "Create a guest, then a room, then complete your booking.\n\n";
          }

          public void AddLog(string message)
          {
               var lines = LogOutput.Split('\n').ToList();
               lines.Add(message);

               if (lines.Count > MaxLines)
                    lines = lines.Skip(lines.Count - MaxLines).ToList();

               LogOutput = string.Join("\n", lines);
               _logger.Info(message);
          }

          public void ClearLog()
          {
               LogOutput = $"Log cleared at {DateTime.Now:HH:mm:ss}.\n\n";
          }
     }
}