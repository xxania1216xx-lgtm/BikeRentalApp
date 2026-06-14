using BikeRentalApp.Models;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BikeRentalApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private string _login = "";
        private string _password = "";
        private string _statusMessage = "";

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            LoginCommand = new RelayCommand(_ => ExecuteLogin());
            RegisterCommand = new RelayCommand(_ => ExecuteRegister());
        }

        private void ExecuteLogin()
        {
            using var db = new BikeRentalDbContext();
            var user = db.Users.FirstOrDefault(u => u.Login == Login && u.Password == Password);

            if (user != null)
            {
                if (user.IsAdmin)
                    _mainViewModel.NavigateToAdminDashboard(user);
                else
                    _mainViewModel.NavigateToUserDashboard(user);
            }
            else
            {
                StatusMessage = "Niepoprawny login lub hasło.";
            }
        }

        private void ExecuteRegister()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Login i hasło nie mogą być puste.";
                return;
            }

            using var db = new BikeRentalDbContext();
            if (db.Users.Any(u => u.Login == Login))
            {
                StatusMessage = "Użytkownik o takim loginie już istnieje.";
                return;
            }

            var newUser = new User
            {
                UserName = Login, // Default to login
                Login = Login,
                Password = Password,
                IsAdmin = false,
                Balance = 0
            };

            db.Users.Add(newUser);
            db.SaveChanges();
            StatusMessage = "Konto założone! Możesz się zalogować.";
        }
    }
}
