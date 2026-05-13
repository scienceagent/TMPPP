using System;
using System.Collections.Generic;
using System.Text;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.TemplateMethod
{
    public abstract class InvoiceGenerator
    {
        // Template Method — defines the fixed skeleton of the invoicing process
        public string GenerateInvoice(Booking booking, Room room)
        {
            var sb = new StringBuilder();

            OnBeforeGeneration(booking, sb); // Hook

            sb.AppendLine(FormatHeader(booking));
            sb.AppendLine(FormatGuestInfo(booking));
            sb.AppendLine(FormatRoomCharges(booking, room));
            sb.AppendLine(FormatTotal(booking, room));
            sb.AppendLine(FormatFooter());

            OnAfterGeneration(booking, sb); // Hook

            string result = sb.ToString();
            SendToGuest(booking, result);
            
            return result;
        }

        // Abstract steps — MUST be implemented by subclasses
        protected abstract string FormatHeader(Booking booking);
        protected abstract string FormatGuestInfo(Booking booking);
        protected abstract string FormatRoomCharges(Booking booking, Room room);
        protected abstract string FormatTotal(Booking booking, Room room);
        protected abstract void SendToGuest(Booking booking, string content);

        // Concrete step shared by all subclasses
        protected virtual string FormatFooter()
        {
            return "------------------------------------------\n" +
                   "Thank you for staying with us!\n" +
                   "Hotel Booking System - Premium Service\n";
        }

        // Hooks — optional extension points
        protected virtual void OnBeforeGeneration(Booking b, StringBuilder sb) { }
        protected virtual void OnAfterGeneration(Booking b, StringBuilder sb) { }
    }
}
