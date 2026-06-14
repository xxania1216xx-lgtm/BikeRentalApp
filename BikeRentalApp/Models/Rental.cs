using System;
using System.Collections.Generic;

namespace BikeRentalApp.Models;

public partial class Rental
{
    public int RentalId { get; set; }

    public int? UserId { get; set; }

    public int? BikeId { get; set; }

    public int? StartStationId { get; set; }

    public int? EndStationId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public decimal? RentalRate { get; set; }

    public decimal? TotalCost { get; set; }

    public string? PaymentMethod { get; set; }

    public decimal? DepositAmount { get; set; }

    public decimal? RideFee { get; set; }

    public int? PlannedHours { get; set; }

    public virtual Bike? Bike { get; set; }

    public virtual Station? EndStation { get; set; }

    public virtual Station? StartStation { get; set; }

    public virtual User? User { get; set; }
}
