using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NexusOS.DAL.Models;

public partial class NexusOsContext : DbContext
{
    public NexusOsContext()
    {
    }

    public NexusOsContext(DbContextOptions<NexusOsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CatCountry> CatCountries { get; set; }

    public virtual DbSet<CatProvince> CatProvinces { get; set; }

    public virtual DbSet<CatWard> CatWards { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=ASUS;Initial Catalog=NexusOS;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatCountry>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CatProvince>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CatWard>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
