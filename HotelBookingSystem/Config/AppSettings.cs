using System;
using System.IO;
using System.Text.Json;

namespace HotelBookingSystem.Config
{
     // ── Typed config model ────────────────────────────────────────────────────
     public sealed class AppSettings
     {
          public GmailConfig GmailDefaults { get; set; } = new();
          public ReportConfig ReportSettings { get; set; } = new();

          // ── Static loader ─────────────────────────────────────────────────────
          private static AppSettings? _instance;

          public static AppSettings Instance
          {
               get
               {
                    if (_instance != null) return _instance;

                    // Look for appsettings.json next to the executable
                    string path = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

                    if (!File.Exists(path))
                    {
                         // Fallback: current directory (useful during debug)
                         path = "appsettings.json";
                    }

                    if (File.Exists(path))
                    {
                         string json = File.ReadAllText(path);
                         _instance = JsonSerializer.Deserialize<AppSettings>(json,
                             new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                             ?? new AppSettings();
                    }
                    else
                    {
                         _instance = new AppSettings();
                    }

                    return _instance;
               }
          }
     }

     public sealed class GmailConfig
     {
          public string Email { get; set; } = string.Empty;
          public string AppPassword { get; set; } = string.Empty;
          public string DisplayName { get; set; } = "Hotel PMS";
     }

     public sealed class ReportConfig
     {
          public string OutputDirectory { get; set; } =
              Path.Combine(
                  System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                  "rapoarte");
     }
}