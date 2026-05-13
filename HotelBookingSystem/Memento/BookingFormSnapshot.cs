using System;

namespace HotelBookingSystem.Memento
{
     // --------------------------------------------------------------------------
     // MEMENTO - BookingFormSnapshot
     // Aceasta clasa este practic un recipient inchis etans in care incap absolut toate datele din formular.
     // Toate constantele devin imutabile si nu pot fi adaugate la o instanta decat o singura data, la initiere.
     // Respecta principiul Memento pentru ca pazeste variabilele limitand accesul cu un `internal`.
     // --------------------------------------------------------------------------
     public class BookingFormSnapshot
     {
          // -- Metadata (visible to Caretaker for display) -----------------------
          public string Label { get; }      // auto-generated or user-named
          public DateTime SavedAt { get; }
          public int StepIndex { get; }      // which wizard step was active

          // -- Form state (accessible only via Originator.Restore) --------------
          // These are effectively "protected" — only Originator reads them via
          // the internal accessor properties below.

          internal string GuestId { get; }
          internal string GuestName { get; }
          internal string GuestEmail { get; }
          internal string GuestNationality { get; }
          internal string GuestPassport { get; }
          internal string RoomId { get; }
          internal string RoomNumber { get; }
          internal string RoomType { get; }
          internal decimal RoomPrice { get; }
          internal int RoomCapacity { get; }
          internal DateTime CheckIn { get; }
          internal DateTime CheckOut { get; }
          internal string BookingType { get; }
          internal bool BreakfastIncluded { get; }
          internal bool AirportTransfer { get; }
          internal string SpecialRequest { get; }
          internal string Notes { get; }

          // -- Internal constructor — ONLY BookingFormOriginator may call this ---
          internal BookingFormSnapshot(
              string label,
              int stepIndex,
              string guestId,
              string guestName,
              string guestEmail,
              string guestNationality,
              string guestPassport,
              string roomId,
              string roomNumber,
              string roomType,
              decimal roomPrice,
              int roomCapacity,
              DateTime checkIn,
              DateTime checkOut,
              string bookingType,
              bool breakfastIncluded,
              bool airportTransfer,
              string specialRequest,
              string notes)
          {
               Label = label;
               SavedAt = DateTime.Now;
               StepIndex = stepIndex;
               GuestId = guestId;
               GuestName = guestName;
               GuestEmail = guestEmail;
               GuestNationality = guestNationality;
               GuestPassport = guestPassport;
               RoomId = roomId;
               RoomNumber = roomNumber;
               RoomType = roomType;
               RoomPrice = roomPrice;
               RoomCapacity = roomCapacity;
               CheckIn = checkIn;
               CheckOut = checkOut;
               BookingType = bookingType;
               BreakfastIncluded = breakfastIncluded;
               AirportTransfer = airportTransfer;
               SpecialRequest = specialRequest;
               Notes = notes;
          }

          // -- Display helpers (Caretaker-visible) ------------------------------
          public string TimestampFmt => SavedAt.ToString("HH:mm:ss");

          public string Summary =>
              $"Step {StepIndex + 1}: {Label}" +
              (string.IsNullOrEmpty(GuestName) ? "" : $" · {GuestName}") +
              (string.IsNullOrEmpty(RoomNumber) ? "" : $" · Room {RoomNumber}") +
              (CheckOut > CheckIn ? $" · {(CheckOut - CheckIn).Days}n" : "");

          // How many seconds ago was this snapshot taken
          public string AgeLabel
          {
               get
               {
                    var age = DateTime.Now - SavedAt;
                    if (age.TotalSeconds < 60) return $"{(int)age.TotalSeconds}s ago";
                    if (age.TotalMinutes < 60) return $"{(int)age.TotalMinutes}m ago";
                    return $"{(int)age.TotalHours}h ago";
               }
          }

          public override string ToString() => $"[{TimestampFmt}] {Summary}";
     }
}

