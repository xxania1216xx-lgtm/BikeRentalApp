using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BikeRentalApp.Models;

public partial class BikeRentalDbContext : DbContext
{
    public BikeRentalDbContext()
    {
    }

    public BikeRentalDbContext(DbContextOptions<BikeRentalDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bike> Bikes { get; set; }

    public virtual DbSet<Rental> Rentals { get; set; }

    public virtual DbSet<Station> Stations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=BikeRental.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bike>(entity =>
        {
            entity.HasKey(e => e.BikeId);
            entity.ToTable("BIKE");

            entity.Property(e => e.BikeId).HasColumnName("BikeID");
            entity.Property(e => e.CurrentStationId).HasColumnName("CurrentStationID");
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.PricePerHour).HasDefaultValue(15.00m);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Available");

            entity.HasOne(d => d.CurrentStation).WithMany(p => p.Bikes)
                .HasForeignKey(d => d.CurrentStationId);
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.RentalId);
            entity.ToTable("RENTAL");

            entity.Property(e => e.RentalId).HasColumnName("RentalID");
            entity.Property(e => e.BikeId).HasColumnName("BikeID");
            entity.Property(e => e.EndStationId).HasColumnName("EndStationID");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.StartStationId).HasColumnName("StartStationID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Bike).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.BikeId);

            entity.HasOne(d => d.EndStation).WithMany(p => p.RentalEndStations)
                .HasForeignKey(d => d.EndStationId);

            entity.HasOne(d => d.StartStation).WithMany(p => p.RentalStartStations)
                .HasForeignKey(d => d.StartStationId);

            entity.HasOne(d => d.User).WithMany(p => p.Rentals)
                .HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.StationId);
            entity.ToTable("STATION");

            entity.Property(e => e.StationId).HasColumnName("StationID");
            entity.Property(e => e.StationIsActive).HasDefaultValue(true);
            entity.Property(e => e.StationName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.ToTable("USER");

            entity.HasIndex(e => e.Login).IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.Balance).HasDefaultValue(0m);
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
