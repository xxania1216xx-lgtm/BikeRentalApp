using System;
using System.Collections.Generic;

namespace BikeRentalApp.Models;

public partial class Station
{
    public int StationId { get; set; }

    public string StationName { get; set; } = null!;

    public bool? StationIsActive { get; set; }

    public virtual ICollection<Bike> Bikes { get; set; } = new List<Bike>();

    public virtual ICollection<Rental> RentalEndStations { get; set; } = new List<Rental>();

    public virtual ICollection<Rental> RentalStartStations { get; set; } = new List<Rental>();
}
