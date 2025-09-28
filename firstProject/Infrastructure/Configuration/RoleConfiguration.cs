using Infrastructure.Identites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasData(new Role
            {
                Id = 1,
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
            builder.HasData(new Role
            {
                Id = 2,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
            builder.HasData(new Role
            {
                Id = 3,
                Name = "Manager",
                NormalizedName = "Manager".ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }); builder.HasData(new Role
            {
                Id = 4,
                Name = "Account",
                NormalizedName = "Account".ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }); builder.HasData(new Role
            {
                Id = 5,
                Name = "Broker",
                NormalizedName = "Broker".ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
            builder.HasData(new Role
            {
                Id = 6,
                Name = "Company",
                NormalizedName = "COMPANY",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
            builder.HasData(new Role
            {
                Id = 7,
                Name = "CustomerService",
                NormalizedName = "CustomerService".ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
            builder.HasData(new Role
            {
                Id = 8,
                Name = "Saber",
                NormalizedName = "Saber".ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });

        }
    }
}
