using System;
using System.Collections.Generic;
using DomainTracker.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainTracker.DataAccess.Context;

public partial class DomainTrackerDbContext : DbContext
{
    public DomainTrackerDbContext(DbContextOptions<DomainTrackerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Domain> Domains { get; set; }

    public virtual DbSet<FavoriteDomain> FavoriteDomains { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Domains__3214EC07C5BA0C94");

            entity.HasIndex(e => e.Name, "UQ_Domains_Name").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<FavoriteDomain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Favorite__3214EC07560233AE");

            entity.HasIndex(e => new { e.UserId, e.DomainId }, "UQ_FavoriteDomains_UserId_DomainId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Domain).WithMany(p => p.FavoriteDomains)
                .HasForeignKey(d => d.DomainId)
                .HasConstraintName("FK_FavoriteDomains_Domains");

            entity.HasOne(d => d.User).WithMany(p => p.FavoriteDomains)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_FavoriteDomains_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07F08C1CBA");

            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
