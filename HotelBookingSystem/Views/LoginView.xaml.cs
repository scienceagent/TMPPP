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
                vm.Password = pb.Password;
            }
        }
    }
}