using System.Windows;
using HotelBookingSystem.Data;

namespace HotelBookingSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
                    
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
                context.EnsureSeedData();
            }
        }
    }
}
