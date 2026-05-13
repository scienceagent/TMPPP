using System.Linq;
using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Models;
using HotelBookingSystem.Models.User;

namespace HotelBookingSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "hotel_system.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>().HasDiscriminator<string>("RoomType")
                .HasValue<StandardRoom>("Standard")
                .HasValue<DeluxeRoom>("Deluxe")
                .HasValue<Suite>("Suite");

            modelBuilder.Entity<User>().HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Admin>("Admin")
                .HasValue<Guest>("Guest");

            // SQLite does not support List<string> natively. Add converters.
            modelBuilder.Entity<Admin>()
                .Property(a => a.Permissions)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', System.StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        }

        public void EnsureSeedData()
        {
            if (!this.Users.Any())
            {
                this.Users.Add(new Admin(
                    System.Guid.NewGuid().ToString(),
                    "System Administrator",
                    "admin@hotel.com",
                    "555-0100",
                    "admin",
                    "admin",
                    "Administrator",
                    "IT",
                    new System.Collections.Generic.List<string> { "all" }
                ));
            }

            if (!this.Rooms.Any())
            {
                this.Rooms.AddRange(
                    new StandardRoom(System.Guid.NewGuid().ToString(), "101", 100, 2),
                    new StandardRoom(System.Guid.NewGuid().ToString(), "102", 100, 2),
                    new DeluxeRoom(System.Guid.NewGuid().ToString(), "201", 200, 4, new System.Collections.Generic.List<string> { "Wi-Fi" }, true),
                    new Suite(System.Guid.NewGuid().ToString(), "301", 300, 4, true, true)
                );
            }
            this.SaveChanges();
        }
    }
}
