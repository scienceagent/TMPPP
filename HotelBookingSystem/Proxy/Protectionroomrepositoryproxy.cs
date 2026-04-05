using System;
using System.Collections.Generic;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Proxy
{
     /// <summary>
     /// Hotel staff roles — defines what operations each role is allowed to perform.
     /// </summary>
     public enum StaffRole
     {
          FrontDesk,      // can read rooms, cannot save
          HouseKeeping,   // can read all rooms, cannot save
          Manager,        // full access
          ReadOnly        // find by id only
     }

     /// <summary>
     /// Proxy 2 — Protection Proxy for IRoomRepository.
     /// Checks the caller's StaffRole before forwarding to the real repository.
     /// The client never knows the proxy exists — it only sees IRoomRepository.
     /// Proxy vs Decorator distinction:
     ///   - Decorator: adds behavior to an object the CLIENT created
     ///   - Proxy: controls ACCESS to the object — the proxy creates/manages the subject
     /// </summary>
     public class ProtectionRoomRepositoryProxy : IRoomRepository
     {
          private readonly IRoomRepository _real;
          private readonly StaffRole _callerRole;
          private readonly List<string> _log;

          public ProtectionRoomRepositoryProxy(IRoomRepository real, StaffRole callerRole,
                                               List<string> log)
          {
               _real = real;
               _callerRole = callerRole;
               _log = log;
          }

          public Room FindById(string id)
          {
               // All roles can look up a room by ID
               _log.Add($"[Proxy:Auth] {_callerRole} → FindById({id[..8]}...) — ALLOWED");
               return _real.FindById(id);
          }

          public List<Room> GetAvailableRooms()
          {
               EnsureRole("GetAvailableRooms", StaffRole.FrontDesk, StaffRole.HouseKeeping, StaffRole.Manager);
               return _real.GetAvailableRooms();
          }

          public List<Room> GetAllRooms()
          {
               EnsureRole("GetAllRooms", StaffRole.HouseKeeping, StaffRole.Manager);
               return _real.GetAllRooms();
          }

          public void Save(Room room)
          {
               EnsureRole("Save", StaffRole.Manager);  // Only managers can modify rooms
               _real.Save(room);
          }

          private void EnsureRole(string operation, params StaffRole[] allowedRoles)
          {
               foreach (var role in allowedRoles)
               {
                    if (_callerRole == role)
                    {
                         _log.Add($"[Proxy:Auth] {_callerRole} → {operation} — ALLOWED");
                         return;
                    }
               }

               _log.Add($"[Proxy:Auth] {_callerRole} → {operation} — DENIED ✗ (requires: {string.Join(" or ", allowedRoles)})");
               throw new UnauthorizedAccessException(
                   $"Role '{_callerRole}' is not permitted to perform '{operation}'. " +
                   $"Required: {string.Join(" or ", allowedRoles)}.");
          }
     }
}