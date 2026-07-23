using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProtectBroker.Core.Entities;
using Serilog;

namespace ProtectBroker.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for Protect Broker.
/// Manages all database operations and entity relationships.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ILogger _logger = Log.ForContext<ApplicationDbContext>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // Core entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<Relay> Relays => Set<Relay>();
    public DbSet<RelayCommand> RelayCommands => Set<RelayCommand>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<RuleExecution> RuleExecutions => Set<RuleExecution>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Add sensitive data logging in development
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.NormalizedEmail);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
        });

        // Configure Zones
        modelBuilder.Entity<Zone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(128).IsRequired();
            entity.HasMany(z => z.Devices)
                .WithOne(d => d.Zone)
                .HasForeignKey(d => d.ZoneId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Devices
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProtectDeviceId).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.HasMany(d => d.Sensors)
                .WithOne(s => s.Device)
                .HasForeignKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(d => d.Relays)
                .WithOne(r => r.Device)
                .HasForeignKey(r => r.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Sensors
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.HasMany(s => s.Readings)
                .WithOne(r => r.Sensor)
                .HasForeignKey(r => r.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SensorReadings - cleanup old readings
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SensorId, e.Timestamp });
        });

        // Configure Relays
        modelBuilder.Entity<Relay>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProtectRelayId).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.HasMany(r => r.Commands)
                .WithOne(c => c.Relay)
                .HasForeignKey(c => c.RelayId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RelayCommands - audit trail
        modelBuilder.Entity<RelayCommand>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RelayId, e.RequestedAt });
        });

        // Configure Rules
        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.ConditionsJson).HasColumnType("jsonb");
            entity.Property(e => e.ActionsJson).HasColumnType("jsonb");
        });

        // Configure RuleExecutions - execution history
        modelBuilder.Entity<RuleExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RuleId, e.ExecutedAt });
            entity.Property(e => e.ExecutionLog).HasColumnType("jsonb");
        });

        // Configure AuditLogs
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.ActionType, e.Timestamp });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.Property(e => e.Changes).HasColumnType("jsonb");
        });

        // Configure Settings
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(128);
        });

        // Configure Notifications
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(512).IsRequired();
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });
    }
}
