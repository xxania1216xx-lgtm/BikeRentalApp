using System;
using System.Collections.Generic;

namespace BikeRentalApp.Models;

public partial class Bike
{
    public int BikeId { get; set; }

    public string Model { get; set; } = null!;

    public decimal PricePerHour { get; set; }

    public string? Status { get; set; }

    public int? CurrentStationId { get; set; }

    public virtual Station? CurrentStation { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
