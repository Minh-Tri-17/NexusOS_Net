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

    public virtual DbSet<CatContractType> CatContractTypes { get; set; }

    public virtual DbSet<CatCountry> CatCountries { get; set; }

    public virtual DbSet<CatDepartment> CatDepartments { get; set; }

    public virtual DbSet<CatLeaveType> CatLeaveTypes { get; set; }

    public virtual DbSet<CatProvince> CatProvinces { get; set; }

    public virtual DbSet<CatWard> CatWards { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VwRandomBinary> VwRandomBinaries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=ASUS;Initial Catalog=NexusOS;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatContractType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Cat_ ContractTypes");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CatCountry>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CatDepartment>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CatLeaveType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CatProvince>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Country).WithMany(p => p.CatProvinces).HasConstraintName("FK_Cat_Provinces_Cat_Countries");
        });

        modelBuilder.Entity<CatWard>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Province).WithMany(p => p.CatWards).HasConstraintName("FK_Cat_Wards_Cat_Provinces");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ContractType).WithMany(p => p.Contracts).HasConstraintName("FK_Contracts_Cat_ContractTypes");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Country).WithMany(p => p.Employees).HasConstraintName("FK_Employees_Cat_Countries");

            entity.HasOne(d => d.Province).WithMany(p => p.Employees).HasConstraintName("FK_Employees_Cat_Provinces");

            entity.HasOne(d => d.Ward).WithMany(p => p.Employees).HasConstraintName("FK_Employees_Cat_Wards");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<VwRandomBinary>(entity =>
        {
            entity.ToView("vw_RandomBinary");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
