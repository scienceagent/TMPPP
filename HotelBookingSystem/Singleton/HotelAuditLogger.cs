using System;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Singleton
{
     public sealed partial class HotelAuditLogger : ILogger
     {
          private static readonly Lazy<HotelAuditLogger> _instance =
              new Lazy<HotelAuditLogger>(() => new HotelAuditLogger());

          private HotelAuditLogger() { }
          public static HotelAuditLogger Instance => _instance.Value;

          public void Info(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss} [INFO]  {message}");
          public void Warn(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss} [WARN]  {message}");
          public void Error(string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss} [ERROR] {message}");
     }
}