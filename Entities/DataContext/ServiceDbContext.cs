using DataAccess;
using Entities.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.DataContext
{
    public class ServiceDbContext:DbContext
    {
        public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Battery> Batteries { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
        public DbSet<Status> Status { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<User> userEntity = modelBuilder.Entity<User>();
            userEntity.ToTable("Users");
            userEntity.HasKey(e => e.Id);
            userEntity.Property(e => e.Id).ValueGeneratedOnAdd();
            userEntity.Property(e => e.DateRegistered).IsRequired();
            userEntity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId);

            EntityTypeBuilder<Client> clientEntity = modelBuilder.Entity<Client>();
            clientEntity.ToTable("Clients");
            clientEntity.HasKey(e => e.Id);
            clientEntity.Property(e => e.Id).ValueGeneratedOnAdd();
            clientEntity.Property(e => e.DateRegistered).IsRequired();

            EntityTypeBuilder<Role> roleEntity = modelBuilder.Entity<Role>();
            roleEntity.ToTable("Roles");
            roleEntity.HasKey(e => e.Id);
            roleEntity.Property(e => e.Id).ValueGeneratedOnAdd();

            EntityTypeBuilder<Report> reportEntity = modelBuilder.Entity<Report>();
            reportEntity.ToTable("Reports");
            reportEntity.HasKey(e => e.Id);
            reportEntity.Property(e => e.Id).ValueGeneratedOnAdd();
            reportEntity.HasOne(r => r.Battery)
                        .WithOne(b => b.Report)
                        .HasForeignKey<Report>(u => u.BatteryId);
            reportEntity.HasOne(r => r.Status)
                        .WithMany(r => r.Reports)
                        .HasForeignKey(u => u.StatusId);

            EntityTypeBuilder<Battery> batteryEntity = modelBuilder.Entity<Battery>();
            batteryEntity.ToTable("Batteries");
            batteryEntity.HasKey(e => e.Id);
            batteryEntity.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
            batteryEntity.Property(e => e.DateRegistered).IsRequired();
            batteryEntity.HasOne(u => u.Client)
                         .WithMany(r => r.Batteries)
                         .HasForeignKey(u => u.ClientId)
                         .IsRequired(false);

            EntityTypeBuilder<Measurement> measurementEntity = modelBuilder.Entity<Measurement>();
            measurementEntity.ToTable("Measurements");
            measurementEntity.HasKey(e => e.Id);
            measurementEntity.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
            measurementEntity.Property(e => e.MeasurementDate).IsRequired();
            measurementEntity.HasOne(u => u.Battery)
                             .WithMany(r => r.Measurements)
                             .HasForeignKey(u => u.BatteryId);

            EntityTypeBuilder<Status> statusEntity = modelBuilder.Entity<Status>();
            statusEntity.ToTable("Status");
            statusEntity.HasKey(e => e.Id);
            statusEntity.Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
