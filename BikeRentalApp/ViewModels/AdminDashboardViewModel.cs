using BikeRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
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
        private Station? _selectedStationForEdit;
        public string NewStationName { get => _newStationName; set => SetProperty(ref _newStationName, value); }
        public Station? SelectedStationForEdit { get => _selectedStationForEdit; set => SetProperty(ref _selectedStationForEdit, value); }
        public ObservableCollection<Station> Stations { get; } = new();

        // Bike Management
        private string _newBikeModel = "";
        private decimal _newBikePrice = 15;
        private Station? _targetStation;
        private Bike? _selectedBikeForEdit;
        private int? _selectedBikeStationId;

        public string NewBikeModel { get => _newBikeModel; set => SetProperty(ref _newBikeModel, value); }
        public decimal NewBikePrice { get => _newBikePrice; set => SetProperty(ref _newBikePrice, value); }
        public Station? TargetStation { get => _targetStation; set => SetProperty(ref _targetStation, value); }
        public Bike? SelectedBikeForEdit 
        { 
            get => _selectedBikeForEdit; 
            set 
            { 
                SetProperty(ref _selectedBikeForEdit, value);
                if (value != null) SelectedBikeStationId = value.CurrentStationId;
            } 
        }
        public int? SelectedBikeStationId { get => _selectedBikeStationId; set => SetProperty(ref _selectedBikeStationId, value); }
        public ObservableCollection<Bike> AllBikes { get; } = new();

        // User Management
        private string _newUserLogin = "";
        private string _newUserPassword = "";
        private string _newUserName = "";
        private bool _newUserIsAdmin;
        private User? _selectedUserForEdit;
        public string NewUserLogin { get => _newUserLogin; set => SetProperty(ref _newUserLogin, value); }
        public string NewUserPassword { get => _newUserPassword; set => SetProperty(ref _newUserPassword, value); }
        public string NewUserName { get => _newUserName; set => SetProperty(ref _newUserName, value); }
        public bool NewUserIsAdmin { get => _newUserIsAdmin; set => SetProperty(ref _newUserIsAdmin, value); }
        public User? SelectedUserForEdit { get => _selectedUserForEdit; set => SetProperty(ref _selectedUserForEdit, value); }
        public ObservableCollection<User> AllUsers { get; } = new();

        // Admin Profile
        private string _newPassword = "";
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        // Commands
        public ICommand AddStationCommand { get; }
        public ICommand UpdateStationCommand { get; }
        public ICommand DeleteStationCommand { get; }

        public ICommand AddBikeCommand { get; }
        public ICommand UpdateBikeCommand { get; }
        public ICommand DeleteBikeCommand { get; }

        public ICommand AddUserCommand { get; }
        public ICommand UpdateUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel(MainViewModel mainViewModel, User user)
        {
            _mainViewModel = mainViewModel;
            _user = user;

            AddStationCommand = new RelayCommand(_ => ExecuteAddStation());
            UpdateStationCommand = new RelayCommand(_ => ExecuteUpdateStation(), _ => SelectedStationForEdit != null);
            DeleteStationCommand = new RelayCommand(_ => ExecuteDeleteStation(), _ => SelectedStationForEdit != null);

            AddBikeCommand = new RelayCommand(_ => ExecuteAddBike());
            UpdateBikeCommand = new RelayCommand(_ => ExecuteUpdateBike(), _ => SelectedBikeForEdit != null);
            DeleteBikeCommand = new RelayCommand(_ => ExecuteDeleteBike(), _ => SelectedBikeForEdit != null);

            AddUserCommand = new RelayCommand(_ => ExecuteAddUser());
            UpdateUserCommand = new RelayCommand(_ => ExecuteUpdateUser(), _ => SelectedUserForEdit != null);
            DeleteUserCommand = new RelayCommand(_ => ExecuteDeleteUser(), _ => SelectedUserForEdit != null && SelectedUserForEdit.UserId != _user.UserId);

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

            var bikes = db.Bikes.Include(b => b.CurrentStation).ToList();
            AllBikes.Clear();
            foreach (var b in bikes) AllBikes.Add(b);

            var users = db.Users.ToList();
            AllUsers.Clear();
            foreach (var u in users) AllUsers.Add(u);
        }

        #region Station Methods
        private void ExecuteAddStation()
        {
            if (string.IsNullOrWhiteSpace(NewStationName)) return;
            using var db = new BikeRentalDbContext();
            db.Stations.Add(new Station { StationName = NewStationName, StationIsActive = true });
            db.SaveChanges();
            StatusMessage = $"Dodano stację: {NewStationName}";
            NewStationName = "";
            LoadData();
        }

        private void ExecuteUpdateStation()
        {
            if (SelectedStationForEdit == null) return;
            using var db = new BikeRentalDbContext();
            var station = db.Stations.Find(SelectedStationForEdit.StationId);
            if (station != null)
            {
                station.StationName = SelectedStationForEdit.StationName;
                station.StationIsActive = SelectedStationForEdit.StationIsActive;
                db.SaveChanges();
                StatusMessage = "Zaktualizowano stację.";
                LoadData();
            }
        }

        private void ExecuteDeleteStation()
        {
            if (SelectedStationForEdit == null) return;
            using var db = new BikeRentalDbContext();
            var station = db.Stations.Find(SelectedStationForEdit.StationId);
            if (station == null) return;

            var otherStation = db.Stations.FirstOrDefault(s => s.StationId != station.StationId);
            if (otherStation == null)
            {
                StatusMessage = "Nie można usunąć ostatniej stacji.";
                return;
            }

            var bikesAtStation = db.Bikes.Where(b => b.CurrentStationId == station.StationId).ToList();
            foreach (var b in bikesAtStation) b.CurrentStationId = otherStation.StationId;

            db.Stations.Remove(station);
            db.SaveChanges();
            StatusMessage = $"Usunięto stację. Rowery przeniesiono do: {otherStation.StationName}";
            SelectedStationForEdit = null;
            LoadData();
        }
        #endregion

        #region Bike Methods
        private void ExecuteAddBike()
        {
            if (string.IsNullOrWhiteSpace(NewBikeModel) || TargetStation == null)
            {
                StatusMessage = "Podaj model i wybierz stację.";
                return;
            }
            using var db = new BikeRentalDbContext();
            db.Bikes.Add(new Bike { Model = NewBikeModel, PricePerHour = NewBikePrice, Status = "Available", CurrentStationId = TargetStation.StationId });
            db.SaveChanges();
            StatusMessage = $"Dodano rower: {NewBikeModel}";
            NewBikeModel = "";
            LoadData();
        }

        private void ExecuteUpdateBike()
        {
            if (SelectedBikeForEdit == null) return;
            using var db = new BikeRentalDbContext();
            var bike = db.Bikes.Find(SelectedBikeForEdit.BikeId);
            if (bike != null)
            {
                bike.Model = SelectedBikeForEdit.Model;
                bike.PricePerHour = SelectedBikeForEdit.PricePerHour;
                bike.CurrentStationId = SelectedBikeStationId;
                db.SaveChanges();
                StatusMessage = "Zaktualizowano dane roweru.";
                LoadData();
            }
        }

        private void ExecuteDeleteBike()
        {
            if (SelectedBikeForEdit == null) return;
            using var db = new BikeRentalDbContext();
            var bike = db.Bikes.Find(SelectedBikeForEdit.BikeId);
            if (bike == null) return;

            var activeRental = db.Rentals.FirstOrDefault(r => r.BikeId == bike.BikeId && r.EndTime == null);
            if (activeRental != null)
            {
                activeRental.EndTime = DateTime.Now;
                activeRental.TotalCost = 0; 
                StatusMessage = "Usunięto rower i przerwano aktywne wypożyczenie.";
            }
            else
            {
                StatusMessage = "Usunięto rower.";
            }

            db.Bikes.Remove(bike);
            db.SaveChanges();
            SelectedBikeForEdit = null;
            LoadData();
        }
        #endregion

        #region User Methods
        private void ExecuteAddUser()
        {
            if (string.IsNullOrWhiteSpace(NewUserLogin)) return;
            using var db = new BikeRentalDbContext();
            if (db.Users.Any(u => u.Login == NewUserLogin)) { StatusMessage = "Login zajęty."; return; }
            db.Users.Add(new User { UserName = NewUserName, Login = NewUserLogin, Password = NewUserPassword, IsAdmin = NewUserIsAdmin, Balance = 0 });
            db.SaveChanges();
            StatusMessage = "Dodano użytkownika.";
            NewUserLogin = ""; NewUserPassword = ""; NewUserName = "";
            LoadData();
        }

        private void ExecuteUpdateUser()
        {
            if (SelectedUserForEdit == null) return;
            using var db = new BikeRentalDbContext();
            var user = db.Users.Find(SelectedUserForEdit.UserId);
            if (user != null)
            {
                user.UserName = SelectedUserForEdit.UserName;
                user.Login = SelectedUserForEdit.Login;
                user.IsAdmin = SelectedUserForEdit.IsAdmin;
                user.Balance = SelectedUserForEdit.Balance;
                db.SaveChanges();
                StatusMessage = "Zaktualizowano dane użytkownika.";
                LoadData();
            }
        }

        private void ExecuteDeleteUser()
        {
            if (SelectedUserForEdit == null) return;
            using var db = new BikeRentalDbContext();
            var user = db.Users.Find(SelectedUserForEdit.UserId);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
                StatusMessage = "Usunięto użytkownika.";
                SelectedUserForEdit = null;
                LoadData();
            }
        }
        #endregion

        private void ExecuteChangePassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword)) return;
            using var db = new BikeRentalDbContext();
            var admin = db.Users.Find(_user.UserId);
            if (admin != null) { admin.Password = NewPassword; db.SaveChanges(); StatusMessage = "Hasło zmienione."; NewPassword = ""; }
        }
    }
}
