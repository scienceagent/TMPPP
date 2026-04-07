using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Services;

namespace HotelBookingSystem.ViewModels
{
    public class LoginViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private string _username = "";
        private string _errorMessage = "";
        private string _selectedRole;
        private bool _isBusy;
        private SecureString _securePassword;
        private readonly IAuthService _authService;

        private readonly Dictionary<string, List<string>> _errors = new();

        public ObservableCollection<string> Roles { get; }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ValidateUsername();
                    _loginCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    ValidateRole();
                    _loginCommand?.RaiseCanExecuteChanged();
                }
            }
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

        // INotifyDataErrorInfo
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public LoginViewModel() : this(new AuthenticationService()) { }

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            Roles = new ObservableCollection<string>
            {
                "Front Desk",
                "Manager",
                "Housekeeping",
                "Administrator"
            };
            _selectedRole = Roles[0]; // Default selection

            ValidateUsername();
            ValidateRole();
            ValidatePassword();

            _loginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsBusy && !HasErrors);
        }

        public void SetSecurePassword(SecureString securePassword)
        {
            _securePassword?.Dispose();
            _securePassword = securePassword?.Copy();
            ValidatePassword();
            _loginCommand?.RaiseCanExecuteChanged();
        }

        public async Task LoginAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                if (HasErrors)
                {
                    ErrorMessage = "Please fix validation errors.";
                    return;
                }

                bool ok = false;
                try
                {
                    ok = await _authService.AuthenticateAsync(Username, _securePassword, SelectedRole);
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Authentication failed. Try again.";
                    return;
                }

                if (ok)
                {
                    ErrorMessage = string.Empty;
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
            Username = string.Empty;
            ErrorMessage = string.Empty;
            SelectedRole = Roles[0];
            _securePassword?.Dispose();
            _securePassword = null;
            _errors.Clear();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Username)));
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(SelectedRole)));
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs("Password"));
            _loginCommand?.RaiseCanExecuteChanged();
        }

        // Validation helpers
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(kv => kv.Value).Cast<object>().ToList();

            if (_errors.TryGetValue(propertyName, out var list))
                return list.Cast<object>().ToList();

            return Enumerable.Empty<object>();
        }

        private void AddError(string property, string message)
        {
            if (!_errors.TryGetValue(property, out var list))
            {
                list = new List<string>();
                _errors[property] = list;
            }
            if (!list.Contains(message))
            {
                list.Add(message);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
            }
        }

        private void RemoveErrors(string property)
        {
            if (_errors.Remove(property))
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
            }
        }

        private void ValidateUsername()
        {
            RemoveErrors(nameof(Username));
            if (string.IsNullOrWhiteSpace(Username))
                AddError(nameof(Username), "Username is required.");
            else if (Username.Length < 3)
                AddError(nameof(Username), "Username must be at least 3 characters.");
        }

        private void ValidatePassword()
        {
            RemoveErrors("Password");
            if (_securePassword == null || _securePassword.Length == 0)
                AddError("Password", "Password is required.");
        }

        private void ValidateRole()
        {
            RemoveErrors(nameof(SelectedRole));
            if (string.IsNullOrWhiteSpace(SelectedRole))
                AddError(nameof(SelectedRole), "Role is required.");
        }
    }
}