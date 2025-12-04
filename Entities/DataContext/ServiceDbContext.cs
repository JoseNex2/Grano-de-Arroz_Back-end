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
            EntityTypeBuilder<Role> roleEntity = modelBuilder.Entity<Role>();
            roleEntity.ToTable("Roles");
            roleEntity.HasKey(e => e.Id);
            roleEntity.Property(e => e.Id).ValueGeneratedOnAdd();

            EntityTypeBuilder<SecureRandomToken> secureRandomTokenEntity = modelBuilder.Entity<SecureRandomToken>();
            secureRandomTokenEntity.ToTable("SecureRandomTokens");
            secureRandomTokenEntity.HasKey(e => e.Id);
            secureRandomTokenEntity.Property(e => e.Id).ValueGeneratedOnAdd();
            secureRandomTokenEntity.Property(e => e.ExpiredDate).IsRequired();
            secureRandomTokenEntity.Property(e => e.CreatedDate).IsRequired();
            secureRandomTokenEntity.HasOne(u => u.User)
                         .WithMany(r => r.SecureRandomTokens)
                         .HasForeignKey(u => u.UserId);

            EntityTypeBuilder<User> userEntity = modelBuilder.Entity<User>();
            userEntity.ToTable("Users");
            userEntity.HasKey(e => e.Id);
            userEntity.Property(e => e.Id).ValueGeneratedOnAdd();
            userEntity.Property(e => e.RegisteredDate).IsRequired();
            userEntity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId);

            EntityTypeBuilder<Client> clientEntity = modelBuilder.Entity<Client>();
            clientEntity.ToTable("Clients");
            clientEntity.HasKey(e => e.Id);
            clientEntity.Property(e => e.Id).ValueGeneratedOnAdd();
            clientEntity.Property(e => e.RegisteredDate).IsRequired();

            EntityTypeBuilder<Status> statusEntity = modelBuilder.Entity<Status>();
            statusEntity.ToTable("Status");
            statusEntity.HasKey(e => e.Id);
            statusEntity.Property(e => e.Id).ValueGeneratedOnAdd();

            EntityTypeBuilder<MeasurementStatus> measurementStatusEntity = modelBuilder.Entity<MeasurementStatus>();
            measurementStatusEntity.ToTable("MeasurementsStatus");
            measurementStatusEntity.HasKey(e => e.Id);
            measurementStatusEntity.Property(e => e.Id).ValueGeneratedOnAdd();
            measurementStatusEntity.HasOne(r => r.Status)
                        .WithMany(r => r.MeasurementsStatus)
                        .HasForeignKey(u => u.StatusId);
            measurementStatusEntity.HasOne(r => r.Report)
                        .WithMany(r => r.MeasurementsStatus)
                        .HasForeignKey(u => u.ReportId);

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
            batteryEntity.Property(e => e.RegisteredDate).IsRequired();
            batteryEntity.HasOne(u => u.Client)
                         .WithMany(r => r.Batteries)
                         .HasForeignKey(u => u.ClientId);

            EntityTypeBuilder<Measurement> measurementEntity = modelBuilder.Entity<Measurement>();
            measurementEntity.ToTable("Measurements");
            measurementEntity.HasKey(e => e.Id);
            measurementEntity.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
            measurementEntity.Property(e => e.MeasurementDate).IsRequired();
            measurementEntity.HasOne(u => u.Battery)
                             .WithMany(r => r.Measurements)
                             .HasForeignKey(u => u.BatteryId);
        }
    }
}
