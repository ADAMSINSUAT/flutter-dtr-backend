using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sample_DTR_API.Models;

public partial class SampleDtrDbContext : DbContext
{
    public SampleDtrDbContext()
    {
    }

    public SampleDtrDbContext(DbContextOptions<SampleDtrDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<TimeIn> TimeIns { get; set; }

    public virtual DbSet<UserCredential> UserCredentials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
           .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
           .AddJsonFile("appsettings.json")
           .Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("ConnString"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DepartmentName).HasMaxLength(50);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmpId);

            entity.Property(e => e.EmpId).HasColumnName("EmpID");
            entity.Property(e => e.DateOfBirth).HasColumnType("date");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Mi)
                .HasMaxLength(1)
                .HasColumnName("MI");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Departments");

            entity.HasOne(d => d.Role).WithMany(p => p.Employees)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Roles");

            entity.HasOne(d => d.Status).WithMany(p => p.Employees)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_Statuses");

            entity.HasOne(d => d.User).WithMany(p => p.Employees)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_UserCredentials");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Role1)
                .HasMaxLength(50)
                .HasColumnName("Role");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK_Status");

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.Status1)
                .HasMaxLength(8)
                .HasColumnName("Status");
        });

        modelBuilder.Entity<TimeIn>(entity =>
        {
            entity.ToTable("TimeIn");

            entity.Property(e => e.TimeInId).HasColumnName("TimeInID");
            entity.Property(e => e.TimeInAm).HasColumnName("TimeInAM");
            entity.Property(e => e.TimeInDate).HasColumnType("date");
            entity.Property(e => e.TimeInPm).HasColumnName("TimeInPM");
            entity.Property(e => e.TimeOutAm).HasColumnName("TimeOutAM");
            entity.Property(e => e.TimeOutPm).HasColumnName("TimeOutPM");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.TimeIns)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TimeIn_UserCredentials1");
        });

        modelBuilder.Entity<UserCredential>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
