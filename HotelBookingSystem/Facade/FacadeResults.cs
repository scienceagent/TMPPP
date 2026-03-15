using System.Collections.Generic;

namespace HotelBookingSystem.Facade
{
     public class CheckInResult
     {
          public bool Success { get; }
          public string Message { get; }
          public string GuestName { get; }
          public string RoomNumber { get; }
          public decimal AmountCharged { get; }
          public string TransactionId { get; }

          private CheckInResult(bool s, string m, string g = "", string r = "", decimal a = 0, string t = "")
          { Success = s; Message = m; GuestName = g; RoomNumber = r; AmountCharged = a; TransactionId = t; }

          public static CheckInResult Ok(string g, string r, decimal a, string t) => new(true, "OK", g, r, a, t);
          public static CheckInResult Fail(string m) => new(false, m);
     }

     public class CheckOutResult
     {
          public bool Success { get; }
          public string Message { get; }
          public string GuestName { get; }
          public string RoomNumber { get; }
          public decimal ServicesTotal { get; }
          public IReadOnlyList<string> ServiceLines { get; }

          private CheckOutResult(bool s, string m, string g = "", string r = "", decimal st = 0,
              IReadOnlyList<string>? lines = null)
          { Success = s; Message = m; GuestName = g; RoomNumber = r; ServicesTotal = st; ServiceLines = lines ?? new List<string>(); }

          public static CheckOutResult Ok(string g, string r, decimal st, IReadOnlyList<string> lines) => new(true, "OK", g, r, st, lines);
          public static CheckOutResult Fail(string m) => new(false, m);
     }
}