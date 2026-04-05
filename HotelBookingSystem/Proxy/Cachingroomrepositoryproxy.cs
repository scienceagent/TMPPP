using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;

namespace HotelBookingSystem.Proxy
{
     /// <summary>
     /// Proxy 1 — Caching Proxy for IRoomRepository.
     /// Intercepts GetAvailableRooms() and GetAllRooms() and returns cached results
     /// for a configurable TTL (default 30 s).  FindById() is also cached per ID.
     /// Save() invalidates the cache so stale data is never served after a write.
     /// The client (RoomController, HotelFacade) uses IRoomRepository as always —
     /// the proxy is TRANSPARENT.
     /// </summary>
     public class CachingRoomRepositoryProxy : IRoomRepository
     {
          private readonly IRoomRepository _real;
          private readonly TimeSpan _ttl;
          private readonly List<string> _log;

          private List<Room>? _allRoomsCache;
          private List<Room>? _availableRoomsCache;
          private readonly Dictionary<string, (Room room, DateTime expires)> _byIdCache = new();
          private DateTime _allExpires = DateTime.MinValue;
          private DateTime _availExpires = DateTime.MinValue;

          public int CacheHits { get; private set; }
          public int CacheMisses { get; private set; }

          public CachingRoomRepositoryProxy(IRoomRepository real, List<string> log,
                                            int ttlSeconds = 30)
          {
               _real = real;
               _ttl = TimeSpan.FromSeconds(ttlSeconds);
               _log = log;
          }

          public Room FindById(string id)
          {
               if (_byIdCache.TryGetValue(id, out var cached) && DateTime.UtcNow < cached.expires)
               {
                    CacheHits++;
                    _log.Add($"[Proxy:Cache] HIT  FindById({id[..8]}...)");
                    return cached.room;
               }

               CacheMisses++;
               _log.Add($"[Proxy:Cache] MISS FindById({id[..8]}...) — querying repository.");
               var room = _real.FindById(id);
               if (room != null)
                    _byIdCache[id] = (room, DateTime.UtcNow.Add(_ttl));
               return room!;
          }

          public List<Room> GetAvailableRooms()
          {
               if (_availableRoomsCache != null && DateTime.UtcNow < _availExpires)
               {
                    CacheHits++;
                    _log.Add($"[Proxy:Cache] HIT  GetAvailableRooms() — {_availableRoomsCache.Count} rooms from cache.");
                    return new List<Room>(_availableRoomsCache);
               }

               CacheMisses++;
               _log.Add("[Proxy:Cache] MISS GetAvailableRooms() — querying repository.");
               _availableRoomsCache = _real.GetAvailableRooms();
               _availExpires = DateTime.UtcNow.Add(_ttl);
               return new List<Room>(_availableRoomsCache);
          }

          public List<Room> GetAllRooms()
          {
               if (_allRoomsCache != null && DateTime.UtcNow < _allExpires)
               {
                    CacheHits++;
                    _log.Add($"[Proxy:Cache] HIT  GetAllRooms() — {_allRoomsCache.Count} rooms from cache.");
                    return new List<Room>(_allRoomsCache);
               }

               CacheMisses++;
               _log.Add("[Proxy:Cache] MISS GetAllRooms() — querying repository.");
               _allRoomsCache = _real.GetAllRooms();
               _allExpires = DateTime.UtcNow.Add(_ttl);
               return new List<Room>(_allRoomsCache);
          }

          public void Save(Room room)
          {
               // Write-through: invalidate cache so stale data isn't served
               _allRoomsCache = null;
               _availableRoomsCache = null;
               _byIdCache.Remove(room.RoomId);
               _allExpires = DateTime.MinValue;
               _availExpires = DateTime.MinValue;
               _log.Add($"[Proxy:Cache] INVALIDATED after Save(room {room.RoomNumber}).");
               _real.Save(room);
          }

          public string GetStats()
              => $"Cache stats — Hits: {CacheHits}, Misses: {CacheMisses}, " +
                 $"Hit rate: {(CacheHits + CacheMisses == 0 ? 0 : CacheHits * 100 / (CacheHits + CacheMisses))}%";
     }
}