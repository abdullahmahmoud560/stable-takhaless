using CustomerSerrvices.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerSerrvices.ApplicationDbContext
{
    public class DB:DbContext
    {
        public DB(DbContextOptions<DB> options):base(options) { }
        public DbSet<Form> forms { get; set; }
    }
}
