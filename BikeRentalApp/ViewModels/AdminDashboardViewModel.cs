using BikeRentalApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace BikeRentalApp.ViewModels
{
    public class AdminDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private User _user;
        private string _statusMessage = "";

        // Station Management
        private string _newStationName = "";
        public string NewStationName { get => _newStationName; set => SetProperty(ref _newStationName, value); }
        public ObservableCollection<Station> Stations { get; } = new();

        // Bike Management
        private string _newBikeModel = "";
        private decimal _newBikePrice = 15;
        private Station? _selectedStation;
        public string NewBikeModel { get => _newBikeModel; set => SetProperty(ref _newBikeModel, value); }
        public decimal NewBikePrice { get => _newBikePrice; set => SetProperty(ref _newBikePrice, value); }
        public Station? SelectedStation { get => _selectedStation; set => SetProperty(ref _selectedStation, value); }

        // User Management
        private string _newUserLogin = "";
        private string _newUserPassword = "";
        private string _newUserName = "";
        private bool _newUserIsAdmin;
        public string NewUserLogin { get => _newUserLogin; set => SetProperty(ref _newUserLogin, value); }
        public string NewUserPassword { get => _newUserPassword; set => SetProperty(ref _newUserPassword, value); }
        public string NewUserName { get => _newUserName; set => SetProperty(ref _newUserName, value); }
        public bool NewUserIsAdmin { get => _newUserIsAdmin; set => SetProperty(ref _newUserIsAdmin, value); }
        public ObservableCollection<User> AllUsers { get; } = new();

        // Admin Profile
        private string _newPassword = "";
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand AddStationCommand { get; }
        public ICommand AddBikeCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel(MainViewModel mainViewModel, User user)
        {
            _mainViewModel = mainViewModel;
            _user = user;

            AddStationCommand = new RelayCommand(_ => ExecuteAddStation());
            AddBikeCommand = new RelayCommand(_ => ExecuteAddBike());
            AddUserCommand = new RelayCommand(_ => ExecuteAddUser());
            ChangePasswordCommand = new RelayCommand(_ => ExecuteChangePassword());
            LogoutCommand = new RelayCommand(_ => _mainViewModel.NavigateToLogin());

            LoadData();
        }

        private void LoadData()
        {
            using var db = new BikeRentalDbContext();
            
            var stations = db.Stations.ToList();
            Stations.Clear();
            foreach (var s in stations) Stations.Add(s);

            var users = db.Users.ToList();
            AllUsers.Clear();
            foreach (var u in users) AllUsers.Add(u);
        }

        private void ExecuteAddStation()
        {
            if (string.IsNullOrWhiteSpace(NewStationName)) return;

            using var db = new BikeRentalDbContext();
            var station = new Station { StationName = NewStationName, StationIsActive = true };
            db.Stations.Add(station);
            db.SaveChanges();

            StatusMessage = $"Dodano stację: {NewStationName}";
            NewStationName = "";
            LoadData();
        }

        private void ExecuteAddBike()
        {
            if (string.IsNullOrWhiteSpace(NewBikeModel) || SelectedStation == null)
            {
                StatusMessage = "Wypełnij model i wybierz stację.";
                return;
            }

            using var db = new BikeRentalDbContext();
            var bike = new Bike
            {
                Model = NewBikeModel,
                PricePerHour = NewBikePrice,
                Status = "Available",
                CurrentStationId = SelectedStation.StationId
            };
            db.Bikes.Add(bike);
            db.SaveChanges();

            StatusMessage = $"Dodano rower: {NewBikeModel}";
            NewBikeModel = "";
            LoadData();
        }

        private void ExecuteAddUser()
        {
            if (string.IsNullOrWhiteSpace(NewUserLogin) || string.IsNullOrWhiteSpace(NewUserPassword))
            {
                StatusMessage = "Wypełnij login i hasło.";
                return;
            }

            using var db = new BikeRentalDbContext();
            if (db.Users.Any(u => u.Login == NewUserLogin))
            {
                StatusMessage = "Użytkownik o takim loginie już istnieje.";
                return;
            }

            var newUser = new User
            {
                UserName = string.IsNullOrWhiteSpace(NewUserName) ? NewUserLogin : NewUserName,
                Login = NewUserLogin,
                Password = NewUserPassword,
                IsAdmin = NewUserIsAdmin,
                Balance = 0
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            StatusMessage = $"Dodano użytkownika: {NewUserLogin}";
            NewUserLogin = "";
            NewUserPassword = "";
            NewUserName = "";
            NewUserIsAdmin = false;
            LoadData();
        }

        private void ExecuteChangePassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword)) return;

            using var db = new BikeRentalDbContext();
            var admin = db.Users.Find(_user.UserId);
            if (admin != null)
            {
                admin.Password = NewPassword;
                db.SaveChanges();
                StatusMessage = "Hasło administratora zmienione.";
                NewPassword = "";
            }
        }
    }
}
