using System.Reflection.Emit;
using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace firstProject.ApplicationDbContext
{
    public class DB : IdentityDbContext <User, IdentityRole, string>
    {
        public DB(DbContextOptions<DB> options) : base(options) { }
        public DbSet<User> users { get; set; }
        public DbSet<TwoFactorVerify> twoFactorVerify { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().HasIndex(e => e.PhoneNumber).IsUnique();

            //builder.Entity<IdentityRole>().HasData(new IdentityRole
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "User",
            //    NormalizedName = "USER",
            //    ConcurrencyStamp = Guid.NewGuid().ToString()
            //});
            //builder.Entity<IdentityRole>().HasData(new IdentityRole
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "Admin",
            //    NormalizedName = "ADMIN",
            //    ConcurrencyStamp = Guid.NewGuid().ToString()
            //}); builder.Entity<IdentityRole>().HasData(new IdentityRole
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "Manager",
            //    NormalizedName = "Manager".ToUpper(),
            //    ConcurrencyStamp = Guid.NewGuid().ToString()
            //});builder.Entity<IdentityRole>().HasData(new IdentityRole
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "Account",
            //    NormalizedName = "Account".ToUpper(),
            //    ConcurrencyStamp = Guid.NewGuid().ToString()
            //}); builder.Entity<IdentityRole>().HasData(new IdentityRole
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "Broker",
            //    NormalizedName = "Broker".ToUpper(),
            //    ConcurrencyStamp = Guid.NewGuid().ToString()
            //});
            //builder.Entity<IdentityRole>().HasData(new IdentityRole
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "Company",
            //    NormalizedName = "COMPANY",
            //    ConcurrencyStamp = Guid.NewGuid().ToString()
            //});
            //builder.Entity<IdentityRole>().HasData(new IdentityRole
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Name = "CustomerService",
            //    NormalizedName = "CustomerService".ToUpper(),
            //    ConcurrencyStamp = Guid.NewGuid().ToString()
            //});
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Saber",
                NormalizedName = "Saber".ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });

        }
    }
}
