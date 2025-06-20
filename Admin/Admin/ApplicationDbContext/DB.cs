using Admin.Model;
using Microsoft.EntityFrameworkCore;

namespace Admin.ApplicationDbContext
{
    public class DB : DbContext
    {
        public DB(DbContextOptions<DB> options) : base(options) { }
        public DbSet<Logs> Logs { get; set; }
    }
}
