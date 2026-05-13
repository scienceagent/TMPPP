using System.Windows;

namespace HotelBookingSystem.Views
{
    public partial class BookingDetailsWindow : Window
    {
        public BookingDetailsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
