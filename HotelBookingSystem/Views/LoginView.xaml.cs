using System.Windows;
using System.Windows.Controls;
using HotelBookingSystem.ViewModels;

namespace HotelBookingSystem.Views
{
    public partial class StaffLoginView : UserControl
    {
        public StaffLoginView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                // Pass the SecureString to the ViewModel (ViewModel will copy/dispose as needed)
                vm.SetSecurePassword(pb.SecurePassword);
            }
        }
    }
}