using System;
using System.Collections.Generic;

namespace BikeRentalApp.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }

    public decimal Balance { get; set; }

    public bool IsAdmin { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
