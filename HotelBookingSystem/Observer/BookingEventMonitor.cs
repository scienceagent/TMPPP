using System;
using System.Collections.Generic;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Observer
{
     // --------------------------------------------------------------------------
     // SUBIECTUL (Observable) - BookingEventMonitor
     // Mentine o lista de ascultatori inregistrati la care trimite mesaje la nivel global.
     // Notifica toti ascultatorii la evenimente specifice prin functia `Notify`.
     // Rezolva el relatiile la inregistrare si trimite pachetul catre toti fara exceptie.
     // --------------------------------------------------------------------------
     public class BookingEventMonitor
     {
          private readonly List<IBookingObserver> _observers = new();

          private readonly IBookingRepository _bookingRepository;
          private readonly IRoomRepository _roomRepository;
          private readonly IUserRepository _userRepository;

          // Audit log forwarded to the activity log panel
          public event Action<string>? OnLog;

          public BookingEventMonitor(
              IBookingRepository bookingRepository,
              IRoomRepository roomRepository,
              IUserRepository userRepository)
          {
               _bookingRepository = bookingRepository;
               _roomRepository = roomRepository;
               _userRepository = userRepository;
          }

          // -- Observer registration ----------------------------------------------

          public void Subscribe(IBookingObserver observer)
          {
               if (!_observers.Contains(observer))
               {
                    _observers.Add(observer);
                    OnLog?.Invoke($"[Observer] Subscribed: {observer.Name}");
               }
          }

          public void Unsubscribe(IBookingObserver observer)
          {
               if (_observers.Remove(observer))
                    OnLog?.Invoke($"[Observer] Unsubscribed: {observer.Name}");
          }

          public IReadOnlyList<IBookingObserver> Observers => _observers;
          public int ObserverCount => _observers.Count;

          // -- Notification entry points ------------------------------------------
          // These are called by MainViewModel whenever a booking state changes.
          // The Subject resolves names from repositories so observers get full data.

          public void NotifyBookingCreated(Booking booking)
              => Notify(BookingEventType.BookingCreated, booking);

          public void NotifyBookingConfirmed(Booking booking)
              => Notify(BookingEventType.BookingConfirmed, booking);

          public void NotifyBookingCancelled(Booking booking)
              => Notify(BookingEventType.BookingCancelled, booking);

          public void NotifyGuestCheckedIn(Booking booking)
              => Notify(BookingEventType.GuestCheckedIn, booking);

          public void NotifyGuestCheckedOut(Booking booking)
              => Notify(BookingEventType.GuestCheckedOut, booking);

          // -- Core dispatch -----------------------------------------------------

          private void Notify(BookingEventType type, Booking booking)
          {
               // Resolve names once — observers receive complete, ready-to-use data
               string guestName = _userRepository.FindById(booking.UserId)?.Name ?? "Unknown Guest";
               var room = _roomRepository.FindById(booking.RoomId);
               string roomNumber = room?.RoomNumber ?? "—";
               decimal basePrice = room?.BasePrice ?? 0m;

               var evt = BookingEvent.From(type, booking, guestName, roomNumber, basePrice);

               OnLog?.Invoke(
                   $"[Observer:Subject] {type} ? firing to {_observers.Count} observer(s)  " +
                   $"[{booking.BookingId[..8]}… · {guestName} · Room {roomNumber}]");

               // Defensive copy — safe if an observer unsubscribes itself during iteration
               var snapshot = new List<IBookingObserver>(_observers);
               foreach (var observer in snapshot)
               {
                    try
                    {
                         observer.OnBookingEvent(evt);
                         OnLog?.Invoke($"  [{observer.Name}] notified ?");
                    }
                    catch (Exception ex)
                    {
                         // One failing observer must never stop the rest from being notified
                         OnLog?.Invoke($"  [{observer.Name}] ERROR: {ex.Message}");
                    }
               }
          }
     }
}

