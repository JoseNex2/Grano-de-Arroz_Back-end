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
        public DbSet<Battery> Batteries { get; set; }

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
<<<<<<< HEAD
            clientEntity.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
            userEntity.Property(e => e.DateRegistered).IsRequired();

            EntityTypeBuilder<Battery> batterytEntity = modelBuilder.Entity<Battery>();
            clientEntity.ToTable("Batteries");
            clientEntity.HasKey(e => e.Id);
            clientEntity.Property(e => e.Id).ValueGeneratedOnAdd().IsRequired();
            userEntity.Property(e => e.DateRegistered).IsRequired();
=======
            clientEntity.Property(e => e.Id).ValueGeneratedOnAdd();
            clientEntity.Property(e => e.DateRegistered).IsRequired();

            EntityTypeBuilder<Role> roleEntity = modelBuilder.Entity<Role>();
            roleEntity.ToTable("Roles");
            roleEntity.HasKey(e => e.Id);
            roleEntity.Property(e => e.Id).ValueGeneratedOnAdd();
>>>>>>> ebeb53cad18c3b61e110395372242c3c154e06ee
        }
    }
}
