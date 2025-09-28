using firstProject.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.ApplicationDbContext
{
    public class RepositoryContextFactory : IDesignTimeDbContextFactory<DB>
    {
        public DB CreateDbContext(string[] args)
        {
            DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", "firstProject", ".env"));

            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Connection");

            var optionsBuilder = new DbContextOptionsBuilder<DB>();
            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                options => options.MigrationsAssembly("Infrastructure"));

            return new DB(optionsBuilder.Options);
        }
    }
}
