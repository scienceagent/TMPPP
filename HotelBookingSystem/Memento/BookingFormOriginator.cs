using System;
using System.Text;

namespace HotelBookingSystem.Memento
{
     // --------------------------------------------------------------------------
     // ORIGINATOR - BookingFormOriginator
     // Ea contine toata starea corenta de pe View unde se desfasoara un Wizard form in UI.
     // Scopul este de a permite doar acestei piese singure din puzzle sa creeze Snapshot(uri).
     // Cand apelam Undo, el este adus aici pentru ca din nou doar el stie cum sa rescrie peste variabile interne.
     // --------------------------------------------------------------------------
     public class BookingFormOriginator
     {
          // -- STEP 1: Guest state ------------------------------------------------
          public string GuestId { get; set; } = "";
          public string GuestName { get; set; } = "";
          public string GuestEmail { get; set; } = "";
          public string GuestNationality { get; set; } = "";
          public string GuestPassport { get; set; } = "";

          // -- STEP 2: Room state -------------------------------------------------
          public string RoomId { get; set; } = "";
          public string RoomNumber { get; set; } = "";
          public string RoomType { get; set; } = "Standard";
          public decimal RoomPrice { get; set; } = 150m;
          public int RoomCapacity { get; set; } = 2;

          // -- STEP 3: Booking details --------------------------------------------
          public DateTime CheckIn { get; set; } = DateTime.Today.AddDays(7);
          public DateTime CheckOut { get; set; } = DateTime.Today.AddDays(9);
          public string BookingType { get; set; } = "Standard";
          public bool BreakfastIncluded { get; set; }
          public bool AirportTransfer { get; set; }
          public string SpecialRequest { get; set; } = "";
          public string Notes { get; set; } = "";

          // -- Current wizard step (0 = Guest, 1 = Room, 2 = Details) -----------
          public int ActiveStep { get; set; }

          // -- Validation --------------------------------------------------------
          public bool IsGuestValid => !string.IsNullOrWhiteSpace(GuestName)
                                     && !string.IsNullOrWhiteSpace(GuestEmail);
          public bool IsRoomValid => !string.IsNullOrWhiteSpace(RoomId)
                                     && RoomPrice > 0;
          public bool IsDatesValid => CheckOut > CheckIn;
          public bool IsFormComplete => IsGuestValid && IsRoomValid && IsDatesValid;

          public int Nights => IsDatesValid ? (CheckOut - CheckIn).Days : 0;

          // -- CREATE MEMENTO — the only way a snapshot is ever produced ---------
          public BookingFormSnapshot Save(string? customLabel = null)
          {
               string label = customLabel ?? AutoLabel();

               return new BookingFormSnapshot(
                   label: label,
                   stepIndex: ActiveStep,
                   guestId: GuestId,
                   guestName: GuestName,
                   guestEmail: GuestEmail,
                   guestNationality: GuestNationality,
                   guestPassport: GuestPassport,
                   roomId: RoomId,
                   roomNumber: RoomNumber,
                   roomType: RoomType,
                   roomPrice: RoomPrice,
                   roomCapacity: RoomCapacity,
                   checkIn: CheckIn,
                   checkOut: CheckOut,
                   bookingType: BookingType,
                   breakfastIncluded: BreakfastIncluded,
                   airportTransfer: AirportTransfer,
                   specialRequest: SpecialRequest,
                   notes: Notes);
          }

          // -- RESTORE FROM MEMENTO — reads the internal snapshot fields ---------
          // Only BookingFormOriginator may access the internal properties
          // of BookingFormSnapshot — enforced by C# `internal` access modifier.
          public void Restore(BookingFormSnapshot snapshot)
          {
               GuestId = snapshot.GuestId;
               GuestName = snapshot.GuestName;
               GuestEmail = snapshot.GuestEmail;
               GuestNationality = snapshot.GuestNationality;
               GuestPassport = snapshot.GuestPassport;
               RoomId = snapshot.RoomId;
               RoomNumber = snapshot.RoomNumber;
               RoomType = snapshot.RoomType;
               RoomPrice = snapshot.RoomPrice;
               RoomCapacity = snapshot.RoomCapacity;
               CheckIn = snapshot.CheckIn;
               CheckOut = snapshot.CheckOut;
               BookingType = snapshot.BookingType;
               BreakfastIncluded = snapshot.BreakfastIncluded;
               AirportTransfer = snapshot.AirportTransfer;
               SpecialRequest = snapshot.SpecialRequest;
               Notes = snapshot.Notes;
               ActiveStep = snapshot.StepIndex;
          }

          // -- Reset to blank draft ----------------------------------------------
          public void Reset()
          {
               GuestId = GuestName = GuestEmail = GuestNationality = GuestPassport = "";
               RoomId = RoomNumber = SpecialRequest = Notes = "";
               RoomType = "Standard";
               BookingType = "Standard";
               RoomPrice = 150m;
               RoomCapacity = 2;
               CheckIn = DateTime.Today.AddDays(7);
               CheckOut = DateTime.Today.AddDays(9);
               BreakfastIncluded = AirportTransfer = false;
               ActiveStep = 0;
          }

          // -- Auto-generate a label describing what's currently filled ----------
          private string AutoLabel()
          {
               var sb = new StringBuilder();

               if (!string.IsNullOrWhiteSpace(GuestName))
                    sb.Append($"Guest: {GuestName.Split(' ')[0]}");

               if (!string.IsNullOrWhiteSpace(RoomNumber))
               {
                    if (sb.Length > 0) sb.Append(" · ");
                    sb.Append($"Room {RoomNumber} ({RoomType})");
               }

               if (IsDatesValid)
               {
                    if (sb.Length > 0) sb.Append(" · ");
                    sb.Append($"{CheckIn:dd MMM}?{CheckOut:dd MMM} ({Nights}n)");
               }

               if (!string.IsNullOrWhiteSpace(BookingType) && BookingType != "Standard")
               {
                    if (sb.Length > 0) sb.Append(" · ");
                    sb.Append(BookingType);
               }

               return sb.Length == 0 ? "Empty draft" : sb.ToString();
          }
     }
}

