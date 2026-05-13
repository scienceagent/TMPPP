using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Services;

namespace HotelBookingSystem.ViewModels
{
    public class SignupViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private string _username = "";
        private string _password = "";
        private string _fullName = "";
        private string _email = "";
        private string _phone = "";
        private string _selectedRole;
        private string _errorMessage = "";
        private bool _isBusy;
        private readonly IAuthService _authService;

        private readonly Dictionary<string, List<string>> _errors = new();

        public ObservableCollection<string> Roles { get; }

        public string Username
        {
            get => _username;
            set { if (SetProperty(ref _username, value)) { ValidateUsername(); _signupCommand?.RaiseCanExecuteChanged(); } }
        }

        public string Password
        {
            get => _password;
            set { if (SetProperty(ref _password, value)) { ValidatePassword(); _signupCommand?.RaiseCanExecuteChanged(); } }
        }

        public string FullName
        {
            get => _fullName;
            set { if (SetProperty(ref _fullName, value)) { ValidateFullName(); _signupCommand?.RaiseCanExecuteChanged(); } }
        }

        public string Email
        {
            get => _email;
            set { if (SetProperty(ref _email, value)) { ValidateEmail(); _signupCommand?.RaiseCanExecuteChanged(); } }
        }

        public string Phone
        {
            get => _phone;
            set { SetProperty(ref _phone, value); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set { if (SetProperty(ref _selectedRole, value)) { _signupCommand?.RaiseCanExecuteChanged(); } }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { if (SetProperty(ref _isBusy, value)) { _signupCommand?.RaiseCanExecuteChanged(); } }
        }

        private RelayCommand _signupCommand;
        public ICommand SignupCommand => _signupCommand;

        public ICommand BackToLoginCommand { get; set; }

        public event Action? OnSignupSuccess;

        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public SignupViewModel() : this(new AuthenticationService()) { }

        public SignupViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            Roles = new ObservableCollection<string>
            {
                "Front Desk",
                "Manager",
                "Housekeeping",
                "Administrator",
                "Guest"
            };
            _selectedRole = Roles[0];

            _signupCommand = new RelayCommand(async _ => await SignupAsync(), _ => !IsBusy && !HasErrors);
        }

        public async Task SignupAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (HasErrors)
                {
                    ErrorMessage = "Please fix validation errors.";
                    return;
                }

                await _authService.RegisterAsync(Username, Password, SelectedRole, FullName, Email, Phone);
                OnSignupSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Database error: {ex.Message}";
                if (ex.InnerException != null) ErrorMessage += $" ({ex.InnerException.Message})";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Clear()
        {
            Username = string.Empty;
            Password = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            ErrorMessage = string.Empty;
            _errors.Clear();
            _signupCommand?.RaiseCanExecuteChanged();
        }

        // INotifyDataErrorInfo implementation
        public IEnumerable GetErrors(string? propertyName)
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
            if (string.IsNullOrWhiteSpace(Username)) AddError(nameof(Username), "Username is required.");
            else if (Username.Length < 3) AddError(nameof(Username), "Min 3 characters.");
        }

        private void ValidatePassword()
        {
            RemoveErrors(nameof(Password));
            if (string.IsNullOrWhiteSpace(Password)) AddError(nameof(Password), "Password is required.");
            else if (Password.Length < 4) AddError(nameof(Password), "Min 4 characters.");
        }

        private void ValidateFullName()
        {
            RemoveErrors(nameof(FullName));
            if (string.IsNullOrWhiteSpace(FullName)) AddError(nameof(FullName), "Full Name is required.");
        }

        private void ValidateEmail()
        {
            RemoveErrors(nameof(Email));
            if (string.IsNullOrWhiteSpace(Email)) AddError(nameof(Email), "Email is required.");
            else if (!Email.Contains("@")) AddError(nameof(Email), "Invalid email format.");
        }
    }
}
