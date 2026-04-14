using System;
using System.Linq;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Command
{
     // ══════════════════════════════════════════════════════════════════════════
     // RECEIVER
     // BookingOperationReceiver knows HOW to perform hotel operations.
     // Concrete commands store a reference to this receiver and delegate their
     // actual work here. The Invoker never touches the Receiver directly.
     //
     // This separates "what to do" (Command) from "how to do it" (Receiver).
     // ══════════════════════════════════════════════════════════════════════════
     public sealed class BookingOperationReceiver
     {
          private readonly IBookingRepository _bookingRepo;
          private readonly IRoomRepository _roomRepo;
          private readonly IBookingService _bookingService;

          public BookingOperationReceiver(
              IBookingRepository bookingRepo,
              IRoomRepository roomRepo,
              IBookingService bookingService)
          {
               _bookingRepo = bookingRepo;
               _roomRepo = roomRepo;
               _bookingService = bookingService;
          }

          // ── Booking operations ─────────────────────────────────────────────

          public void SaveBooking(Booking booking)
          {
               _bookingRepo.Save(booking);
          }

          public void RemoveBooking(string bookingId)
          {
               // InMemoryBookingRepository stores by ID — re-saving with a tombstone
               // is not the right approach; instead we mark it cancelled
               // (true deletion requires a Remove() on the repo).
               // We use a cancelled status as the "removed" state for Undo.
               var booking = _bookingRepo.FindById(bookingId);
               if (booking != null)
               {
                    booking.Cancel();
                    _bookingRepo.Save(booking);
               }
          }

          public void ConfirmBooking(string bookingId)
          {
               _bookingService.ConfirmBooking(bookingId);
          }

          public void RevertBookingToPending(string bookingId)
          {
               // Invert a Confirmed booking back to Pending.
               // We replace the booking object with a fresh Pending copy.
               var existing = _bookingRepo.FindById(bookingId);
               if (existing == null) return;

               var pending = new Booking(
                   existing.BookingId,
                   existing.UserId,
                   existing.RoomId,
                   existing.CheckInDate,
                   existing.CheckOutDate,
                   existing.BookingType);  // default ctor → Pending

               _bookingRepo.Save(pending);

               // Restore room availability
               var room = _roomRepo.FindById(existing.RoomId);
               room?.SetAvailability(true);
               if (room != null) _roomRepo.Save(room);
          }

          public void CancelBooking(string bookingId)
          {
               _bookingService.CancelBooking(bookingId);
          }

          public void RestoreBookingStatus(string bookingId, BookingStatus previousStatus)
          {
               // Revert a cancelled booking to a prior known status
               var existing = _bookingRepo.FindById(bookingId);
               if (existing == null) return;

               var restored = new Booking(
                   existing.BookingId,
                   existing.UserId,
                   existing.RoomId,
                   existing.CheckInDate,
                   existing.CheckOutDate,
                   existing.BookingType);

               // Advance to the target status
               switch (previousStatus)
               {
                    case BookingStatus.Confirmed:
                         restored.Confirm();
                         var room = _roomRepo.FindById(existing.RoomId);
                         room?.SetAvailability(false);
                         if (room != null) _roomRepo.Save(room);
                         break;
                    case BookingStatus.Pending:
                         break; // already Pending after new Booking()
               }

               _bookingRepo.Save(restored);
          }

          // ── Room operations ────────────────────────────────────────────────

          /// <summary>
          /// Returns current base price of the room (for snapshotting before change).
          /// </summary>
          public decimal GetRoomBasePrice(string roomId)
          {
               return _roomRepo.FindById(roomId)?.BasePrice ?? 0m;
          }

          /// <summary>
          /// Adjusts a room's base price by creating a new Room instance
          /// (immutable BasePrice requires replacement).
          /// </summary>
          public void SetRoomBasePrice(string roomId, decimal newPrice)
          {
               var room = _roomRepo.FindById(roomId);
               if (room == null) return;

               // Create replacement with the new price — same type via reflection-free approach
               Room updated = room switch
               {
                    Models.Suite s => new Models.Suite(
                        s.RoomId, s.RoomNumber, newPrice, s.Capacity,
                        s.HasKitchen, s.HasLivingRoom),

                    Models.DeluxeRoom d => new Models.DeluxeRoom(
                        d.RoomId, d.RoomNumber, newPrice, d.Capacity,
                        new System.Collections.Generic.List<string>(d.Amenities), d.HasBalcony),

                    _ => new Models.StandardRoom(
                        room.RoomId, room.RoomNumber, newPrice, room.Capacity)
               };

               updated.SetAvailability(room.IsAvailable);
               _roomRepo.Save(updated);
          }

          // ── Queries (read-only, used by commands to snapshot state) ──────────

          public Booking? FindBooking(string bookingId)
              => _bookingRepo.FindById(bookingId);

          public Room? FindRoom(string roomId)
              => _roomRepo.FindById(roomId);

          public System.Collections.Generic.List<Booking> GetAllBookings()
              => _bookingRepo.GetAllBookings();

          public System.Collections.Generic.List<Room> GetAllRooms()
              => _roomRepo.GetAllRooms();
     }
}