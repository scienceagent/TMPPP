using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelBookingSystem.Commands;
using System.Threading.Tasks;

namespace HotelBookingSystem.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username = "";
        private string _password = "";
        private string _errorMessage = "";
        private string _selectedRole;
        private bool _isBusy;

        public ObservableCollection<string> Roles { get; }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    _loginCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private RelayCommand _loginCommand;
        public ICommand LoginCommand => _loginCommand;

        public event Action? OnLoginSuccess;

        public LoginViewModel()
        {
            Roles = new ObservableCollection<string>
            {
                "Front Desk",
                "Manager",
                "Housekeeping",
                "Administrator"
            };
            _selectedRole = Roles[0]; // Default selection

            _loginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsBusy);
        }

        public async Task LoginAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                // Simple mock authentication for demonstration
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please enter both username and password.";
                    return;
                }

                // Simulate async work (e.g., API call)
                await Task.Delay(50).ConfigureAwait(false);

                if ((Username == "admin" && Password == "admin") ||
                    (Username == "staff" && Password == "staff"))
                {
                    ErrorMessage = "";
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    ErrorMessage = "Invalid credentials for the selected role.";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Clear()
        {
            Username = "";
            Password = "";
            ErrorMessage = "";
            SelectedRole = Roles[0];
        }
    }
}