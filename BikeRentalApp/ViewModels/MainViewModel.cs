using BikeRentalApp.Models;
using System.Linq;

namespace BikeRentalApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;
        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MainViewModel()
        {
            EnsureAdminExists();
            NavigateToLogin();
        }

        private void EnsureAdminExists()
        {
            using var db = new BikeRentalDbContext();
            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                // Add Admin
                db.Users.Add(new User
                {
                    UserName = "Administrator",
                    Login = "admin",
                    Password = "admin",
                    IsAdmin = true,
                    Balance = 0
                });

                // Add 5 Stations
                var stations = new List<Station>();
                for (int i = 1; i <= 5; i++)
                {
                    var station = new Station { StationName = $"Stacja {i}", StationIsActive = true };
                    stations.Add(station);
                }
                db.Stations.AddRange(stations);
                db.SaveChanges(); // Save to get Station IDs

                // Add 10 Bikes
                var bikeModels = new[] { "Kross", "Romet", "Giant", "Scott", "Trek" };
                var random = new Random();
                for (int i = 1; i <= 10; i++)
                {
                    db.Bikes.Add(new Bike
                    {
                        Model = $"{bikeModels[random.Next(bikeModels.Length)]} Model {i}",
                        PricePerHour = 15.00m + random.Next(0, 6),
                        Status = "Available",
                        CurrentStationId = stations[random.Next(stations.Count)].StationId
                    });
                }
                db.SaveChanges();
            }
        }

        public void NavigateToLogin()
        {
            CurrentViewModel = new LoginViewModel(this);
        }

        public void NavigateToUserDashboard(User user)
        {
            CurrentViewModel = new UserDashboardViewModel(this, user);
        }

        public void NavigateToAdminDashboard(User user)
        {
            CurrentViewModel = new AdminDashboardViewModel(this, user);
        }
    }
}
