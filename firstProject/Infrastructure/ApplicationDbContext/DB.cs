

using firstProject.Model;
using Infrastructure.Configuration;
using Infrastructure.Identites;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace firstProject.ApplicationDbContext
{
    public class DB : IdentityDbContext <User, Role, int>
    {
        public DB(DbContextOptions<DB> options) : base(options) { }
        public new DbSet<User> Users { get; set; }
        public DbSet<TwoFactorVerify> twoFactorVerify { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().HasIndex(e => e.PhoneNumber).IsUnique();
            builder.Entity<User>().HasIndex(e => e.Email).IsUnique();
            builder.Entity<User>().HasIndex(e => e.Identity).IsUnique();

            builder.Entity<TwoFactorVerify>(entity =>
            {
                entity.HasKey(t => new { t.UserId, t.TypeOfGenerate });

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            }); ;

            builder.ApplyConfigurationsFromAssembly(typeof(DB).Assembly);
        }
    }
}
