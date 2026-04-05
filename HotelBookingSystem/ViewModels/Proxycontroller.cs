using System;
using System.Collections.ObjectModel;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Proxy;
using System.Collections.Generic;

namespace HotelBookingSystem.ViewModels
{
     public class ProxyController : BaseViewModel
     {
          private readonly IRoomRepository _realRepository;

          // The proxies wrap the SAME real repository
          private CachingRoomRepositoryProxy? _cachingProxy;
          private readonly List<string> _proxyLog = new();

          private string _selectedRole = "FrontDesk";
          private string _cacheResult = "Click 'Test Cache Proxy' to demonstrate caching.";
          private string _authResult = "Click 'Test Auth Proxy' to demonstrate role-based access.";
          private string _cacheStats = "";

          public ObservableCollection<string> Roles { get; } = new()
        {
            "FrontDesk", "HouseKeeping", "Manager", "ReadOnly"
        };

          public string SelectedRole
          {
               get => _selectedRole;
               set => SetProperty(ref _selectedRole, value);
          }

          public string CacheResult
          {
               get => _cacheResult;
               set => SetProperty(ref _cacheResult, value);
          }

          public string AuthResult
          {
               get => _authResult;
               set => SetProperty(ref _authResult, value);
          }

          public string CacheStats
          {
               get => _cacheStats;
               set => SetProperty(ref _cacheStats, value);
          }

          public event Action<string>? OnLog;

          public ProxyController(IRoomRepository realRepository)
          {
               _realRepository = realRepository;
               // Wrap the real repository in the caching proxy once
               _cachingProxy = new CachingRoomRepositoryProxy(realRepository, _proxyLog, ttlSeconds: 30);
          }

          /// <summary>
          /// Demonstrates the Caching Proxy: first call is a MISS (hits DB),
          /// second call is a HIT (served from cache), third call after save is a MISS again.
          /// </summary>
          public void TestCacheProxy()
          {
               _proxyLog.Clear();
               _cachingProxy = new CachingRoomRepositoryProxy(_realRepository, _proxyLog, ttlSeconds: 30);

               OnLog?.Invoke("[Proxy:Cache] ── Caching Proxy Demo ──");

               // Call 1 — MISS
               var rooms1 = _cachingProxy.GetAllRooms();
               OnLog?.Invoke($"  Call 1 GetAllRooms() → {rooms1.Count} rooms");

               // Call 2 — HIT (same TTL window)
               var rooms2 = _cachingProxy.GetAllRooms();
               OnLog?.Invoke($"  Call 2 GetAllRooms() → {rooms2.Count} rooms (from cache)");

               // Call 3 — HIT
               var rooms3 = _cachingProxy.GetAvailableRooms();
               OnLog?.Invoke($"  Call 3 GetAvailableRooms() → {rooms3.Count} rooms");

               // Simulate a save — invalidates cache
               if (rooms1.Count > 0)
               {
                    _cachingProxy.Save(rooms1[0]);
                    OnLog?.Invoke($"  Save(room) → cache INVALIDATED");
               }

               // Call 4 — MISS again after invalidation
               var rooms4 = _cachingProxy.GetAllRooms();
               OnLog?.Invoke($"  Call 4 GetAllRooms() → {rooms4.Count} rooms (cache miss after save)");

               string stats = _cachingProxy.GetStats();
               CacheStats = stats;
               OnLog?.Invoke($"  {stats}\n");

               CacheResult =
                   $"✓ Caching Proxy demonstrated.\n\n" +
                   string.Join("\n", _proxyLog) +
                   $"\n\n{stats}";
          }

          /// <summary>
          /// Demonstrates the Protection Proxy: tests all operations for the selected role.
          /// Some will succeed, others will throw UnauthorizedAccessException.
          /// </summary>
          public void TestAuthProxy()
          {
               var authLog = new List<string>();

               if (!Enum.TryParse<StaffRole>(SelectedRole, out var role))
                    role = StaffRole.FrontDesk;

               var proxy = new ProtectionRoomRepositoryProxy(_realRepository, role, authLog);

               OnLog?.Invoke($"[Proxy:Auth] ── Protection Proxy Demo (Role: {role}) ──");

               // Test FindById
               TryOp(authLog, "FindById",
                   () => {
                        var rooms = _realRepository.GetAllRooms();
                        if (rooms.Count > 0) proxy.FindById(rooms[0].RoomId);
                   });

               // Test GetAvailableRooms
               TryOp(authLog, "GetAvailableRooms",
                   () => proxy.GetAvailableRooms());

               // Test GetAllRooms
               TryOp(authLog, "GetAllRooms",
                   () => proxy.GetAllRooms());

               // Test Save (most restrictive)
               TryOp(authLog, "Save",
                   () => {
                        var rooms = _realRepository.GetAllRooms();
                        if (rooms.Count > 0) proxy.Save(rooms[0]);
                   });

               foreach (var line in authLog)
                    OnLog?.Invoke($"  {line}");
               OnLog?.Invoke("");

               AuthResult =
                   $"✓ Protection Proxy tested for role: {role}\n\n" +
                   string.Join("\n", authLog);
          }

          private void TryOp(List<string> log, string opName, Action op)
          {
               try
               {
                    op();
               }
               catch (UnauthorizedAccessException ex)
               {
                    log.Add($"  → EXCEPTION: {ex.Message}");
               }
          }
     }
}