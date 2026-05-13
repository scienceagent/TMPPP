using System;
using System.Text;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.TemplateMethod
{
    // ─── EMAIL INVOICE ────────────────────────────────────────────────────
    public class EmailInvoiceGenerator : InvoiceGenerator
    {
        protected override string FormatHeader(Booking booking)
        {
            return $"[EMAIL FORMAT] INVOICE #{booking.BookingId.ToUpper()}";
        }

        protected override string FormatGuestInfo(Booking booking)
        {
            return $"Guest ID: {booking.UserId}\nStay: {booking.CheckInDate:yyyy-MM-dd} to {booking.CheckOutDate:yyyy-MM-dd}";
        }

        protected override string FormatRoomCharges(Booking booking, Room room)
        {
            var nights = (booking.CheckOutDate - booking.CheckInDate).TotalDays;
            return $"Accommodation: {room.RoomNumber} ({room.GetType().Name})\n" +
                   $"Rate: {room.BasePrice:C}/night x {nights} nights";
        }

        protected override string FormatTotal(Booking booking, Room room)
        {
            var nights = (booking.CheckOutDate - booking.CheckInDate).TotalDays;
            var total = (decimal)nights * room.BasePrice;
            return $"\n>>> TOTAL DUE: {total:C} (incl. VAT)";
        }

        protected override void SendToGuest(Booking booking, string content)
        {
            Console.WriteLine($"[EmailService] Sending invoice to guest {booking.UserId}...");
        }

        protected override void OnBeforeGeneration(Booking b, StringBuilder sb)
        {
            sb.AppendLine("HTML BEGIN");
            sb.AppendLine("<div style='font-family: Arial;'>");
        }

        protected override void OnAfterGeneration(Booking b, StringBuilder sb)
        {
            sb.AppendLine("</div>");
            sb.AppendLine("HTML END");
        }
    }

    // ─── PDF INVOICE ──────────────────────────────────────────────────────
    public class PdfInvoiceGenerator : InvoiceGenerator
    {
        protected override string FormatHeader(Booking booking)
        {
            return "==========================================\n" +
                   "            OFFICIAL AUDIT INVOICE        \n" +
                   "==========================================";
        }

        protected override string FormatGuestInfo(Booking booking)
        {
            return $"BOOKING REF: {booking.BookingId}\nCLIENT: {booking.UserId}";
        }

        protected override string FormatRoomCharges(Booking booking, Room room)
        {
            var nights = (booking.CheckOutDate - booking.CheckInDate).TotalDays;
            return $"ROOM: {room.RoomNumber}\n" +
                   $"DURATION: {nights} night(s)\n" +
                   $"SUBTOTAL: {(decimal)nights * room.BasePrice:C}";
        }

        protected override string FormatTotal(Booking booking, Room room)
        {
            var nights = (booking.CheckOutDate - booking.CheckInDate).TotalDays;
            return $"\nFINAL AMOUNT: {(decimal)nights * room.BasePrice:C}";
        }

        protected override void SendToGuest(Booking booking, string content)
        {
            Console.WriteLine($"[FileSystem] Saving PDF: invoice_{booking.BookingId}.pdf");
        }
    }
}
