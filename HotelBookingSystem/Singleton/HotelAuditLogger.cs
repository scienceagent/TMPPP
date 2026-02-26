using System;
using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Singleton
{
     // ─── SINGLETON: HotelAuditLogger ─────────────────────────────────
     // Guarantees exactly ONE instance in the entire application.
     // All booking actions are logged through this single point.
     //
     // Matches GoF:
     //   - private constructor  → nobody creates instances directly
     //   - static Instance      → single global access point
     //   - Lazy<T>              → thread-safe, created only when first needed
     //
     // Replaces/upgrades the existing ConsoleLogger in Services/
     // ConsoleLogger still exists for DI — this is the Singleton variant.
     public  sealed partial class HotelAuditLogger : ILogger
     {
          // Lazy<T> — thread-safe, no manual lock needed (recommended C# approach)
          private static readonly Lazy<HotelAuditLogger> _instance =
              new Lazy<HotelAuditLogger>(() => new HotelAuditLogger());

          // Global access point — the only way to get the instance
          public static HotelAuditLogger Instance => _instance.Value;

          // Private constructor — prevents any external instantiation
          private HotelAuditLogger()
          {
               Console.WriteLine("[HotelAuditLogger] Initialized — single instance created.");
          }

          // ─── ILogger implementation ───────────────────────────────────
          public void Info(string message)
          {
               var entry = $"{DateTime.Now:HH:mm:ss} [INFO]  {message}";
               Console.WriteLine(entry);
          }

          public void Warn(string message)
          {
               var entry = $"{DateTime.Now:HH:mm:ss} [WARN]  {message}";
               Console.WriteLine(entry);
          }

          public void Error(string message)
          {
               var entry = $"{DateTime.Now:HH:mm:ss} [ERROR] {message}";
               Console.WriteLine(entry);
          }

          // Bonus: hotel-specific audit method
          public void LogBookingAction(string bookingId, string action)
          {
               var entry = $"{DateTime.Now:HH:mm:ss} [AUDIT] Booking {bookingId} → {action}";
               Console.WriteLine(entry);
          }
     }
}