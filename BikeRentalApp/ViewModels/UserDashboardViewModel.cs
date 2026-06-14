using BikeRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BikeRentalApp.ViewModels
{
    public class UserDashboardViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private User _user;
        private decimal _rechargeAmount;
        private Station? _selectedStation;
        private Station? _returnStation;
        private Bike? _selectedBike;
        private string _newPassword = "";
        private string _statusMessage = "";
        private string _elapsedTime = "";
        private DispatcherTimer _timer;

        public User User { get => _user; set => SetProperty(ref _user, value); }
        public decimal RechargeAmount { get => _rechargeAmount; set => SetProperty(ref _rechargeAmount, value); }
        public Station? SelectedStation { get => _selectedStation; set { SetProperty(ref _selectedStation, value); LoadBikes(); } }
        public Station? ReturnStation { get => _returnStation; set => SetProperty(ref _returnStation, value); }
        public Bike? SelectedBike { get => _selectedBike; set => SetProperty(ref _selectedBike, value); }
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public string ElapsedTime { get => _elapsedTime; set => SetProperty(ref _elapsedTime, value); }

        public ObservableCollection<Station> Stations { get; } = new();
        public ObservableCollection<Bike> AvailableBikes { get; } = new();
        public ObservableCollection<Rental> RentalHistory { get; } = new();
        public Rental? CurrentRental { get; set; }

        public ICommand RechargeCommand { get; }
        public ICommand RentCommand { get; }
        public ICommand ReturnCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }

        public UserDashboardViewModel(MainViewModel mainViewModel, User user)
        {
            _mainViewModel = mainViewModel;
            _user = user;

            RechargeCommand = new RelayCommand(_ => ExecuteRecharge());
            RentCommand = new RelayCommand(_ => ExecuteRent(), _ => SelectedBike != null);
            ReturnCommand = new RelayCommand(_ => ExecuteReturn(), _ => CurrentRental != null);
            ChangePasswordCommand = new RelayCommand(_ => ExecuteChangePassword());
            LogoutCommand = new RelayCommand(_ => _mainViewModel.NavigateToLogin());

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => UpdateElapsedTime();

            LoadStations();
            LoadHistory();
            CheckCurrentRental();
        }

        private void UpdateElapsedTime()
        {
            if (CurrentRental != null && CurrentRental.StartTime.HasValue)
            {
                var duration = DateTime.Now - CurrentRental.StartTime.Value;
                ElapsedTime = $"Upłynęło: {duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
                
                if (duration.TotalHours >= 24)
                {
                    _timer.Stop();
                    CheckCurrentRental();
                }
            }
        }

        private void LoadStations()
        {
            using var db = new BikeRentalDbContext();
            var stations = db.Stations.Where(s => s.StationIsActive == true).ToList();
            Stations.Clear();
            foreach (var s in stations) Stations.Add(s);
        }

        private void LoadBikes()
        {
            AvailableBikes.Clear();
            if (SelectedStation == null) return;

            using var db = new BikeRentalDbContext();
            var bikes = db.Bikes.Where(b => b.CurrentStationId == SelectedStation.StationId && b.Status == "Available").ToList();
            foreach (var b in bikes) AvailableBikes.Add(b);
        }

        private void LoadHistory()
        {
            using var db = new BikeRentalDbContext();
            var history = db.Rentals
                .Include(r => r.Bike)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Where(r => r.UserId == User.UserId)
                .OrderByDescending(r => r.StartTime)
                .ToList();

            RentalHistory.Clear();
            foreach (var r in history) RentalHistory.Add(r);
        }

        private void CheckCurrentRental()
        {
            using var db = new BikeRentalDbContext();
            CurrentRental = db.Rentals
                .Include(r => r.Bike)
                .FirstOrDefault(r => r.UserId == User.UserId && r.EndTime == null);

            if (CurrentRental != null && CurrentRental.StartTime.HasValue)
            {
                var duration = DateTime.Now - CurrentRental.StartTime.Value;
                if (duration.TotalHours >= 24)
                {
                    var user = db.Users.Find(User.UserId);
                    var bike = db.Bikes.Find(CurrentRental.BikeId);
                    
                    CurrentRental.EndTime = CurrentRental.StartTime.Value.AddHours(24); // Cap rental time at 24h
                    CurrentRental.EndStationId = CurrentRental.StartStationId;

                    var hours = 24m;
                    var rentalCost = Math.Round(hours * (CurrentRental.RentalRate ?? 15), 2);
                    CurrentRental.TotalCost = rentalCost + 30m; // 30 PLN penalty
                    
                    if (user != null) user.Balance -= CurrentRental.TotalCost.Value;
                    
                    if (bike != null)
                    {
                        bike.Status = "Available";
                        bike.CurrentStationId = CurrentRental.StartStationId;
                    }
                    
                    db.SaveChanges();
                    
                    if (user != null) User = user;
                    CurrentRental = null;
                    StatusMessage = "Automatyczny zwrot: rower przetrzymany ponad 24h. Doliczono karę 30 zł.";
                    
                    LoadBikes();
                    LoadHistory();
                }
                else
                {
                    _timer.Start();
                    UpdateElapsedTime();
                }
            }
            else
            {
                _timer.Stop();
                ElapsedTime = "";
            }

            OnPropertyChanged(nameof(CurrentRental));
        }

        private void ExecuteRecharge()
        {
            if (RechargeAmount <= 0) return;

            using var db = new BikeRentalDbContext();
            var user = db.Users.Find(User.UserId);
            if (user != null)
            {
                user.Balance += RechargeAmount;
                db.SaveChanges();
                User = user;
                StatusMessage = $"Doładowano konto kwotą {RechargeAmount:N2} zł";
                RechargeAmount = 0;
            }
        }

        private void ExecuteRent()
        {
            if (SelectedBike == null) return;
            if (User.Balance < SelectedBike.PricePerHour) // Check if user can afford at least 1 hour
            {
                StatusMessage = "Niewystarczające fundusze. Wymagane: " + SelectedBike.PricePerHour.ToString("N2") + " zł";
                return;
            }

            if (CurrentRental != null)
            {
                StatusMessage = "Już masz wypożyczony rower.";
                return;
            }

            using var db = new BikeRentalDbContext();
            var bike = db.Bikes.Find(SelectedBike.BikeId);
            if (bike == null || bike.Status != "Available") return;

            var rental = new Rental
            {
                UserId = User.UserId,
                BikeId = bike.BikeId,
                StartStationId = bike.CurrentStationId,
                StartTime = DateTime.Now,
                RentalRate = bike.PricePerHour,
                DepositAmount = 10 // Example deposit
            };

            bike.Status = "Rented";
            bike.CurrentStationId = null;

            db.Rentals.Add(rental);
            db.SaveChanges();

            CheckCurrentRental();
            LoadBikes();
            LoadHistory();
            StatusMessage = $"Wypożyczono rower: {bike.Model}";
        }

        private void ExecuteReturn()
        {
            if (CurrentRental == null || ReturnStation == null)
            {
                StatusMessage = "Wybierz stację zwrotu.";
                return;
            }

            using var db = new BikeRentalDbContext();
            var rental = db.Rentals.Find(CurrentRental.RentalId);
            var bike = db.Bikes.Find(CurrentRental.BikeId);
            var user = db.Users.Find(User.UserId);

            if (rental == null || bike == null || user == null) return;

            rental.EndTime = DateTime.Now;
            rental.EndStationId = ReturnStation.StationId;
            
            if (rental.StartTime.HasValue && rental.EndTime.HasValue)
            {
                var duration = rental.EndTime.Value - rental.StartTime.Value;
                var hours = (decimal)duration.TotalHours;
                if (hours < 0.1m) hours = 0.1m; // Minimum 6 mins charge

                rental.TotalCost = Math.Round(hours * (rental.RentalRate ?? 15), 2);
                user.Balance -= rental.TotalCost.Value;
            }

            bike.Status = "Available";
            bike.CurrentStationId = ReturnStation.StationId;

            db.SaveChanges();

            User = user;
            CurrentRental = null;
            ReturnStation = null;
            _timer.Stop();
            ElapsedTime = "";

            OnPropertyChanged(nameof(CurrentRental));
            OnPropertyChanged(nameof(ReturnStation));
            LoadBikes();
            LoadHistory();
            StatusMessage = $"Zwrócono rower. Koszt: {rental.TotalCost:N2} zł";
        }

        private void ExecuteChangePassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword)) return;

            using var db = new BikeRentalDbContext();
            var user = db.Users.Find(User.UserId);
            if (user != null)
            {
                user.Password = NewPassword;
                db.SaveChanges();
                StatusMessage = "Hasło zostało zmienione.";
                NewPassword = "";
            }
        }
    }
}
