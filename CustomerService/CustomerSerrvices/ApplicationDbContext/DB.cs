using CustomerSerrvices.Models;
using freelancer.Models;
using Microsoft.EntityFrameworkCore;
using User.Model;

namespace CustomerSerrvices.ApplicationDbContext
{
    public class DB:DbContext
    {
        public DB(DbContextOptions<DB> options):base(options) { }
        public DbSet<Form> forms { get; set; }
        public DbSet<saberCertificate> saberCertificates { get; set; }
        public DbSet<ChatMessage> chatMessages { get; set; }
        public DbSet<ChatSummary> chatSummaries { get; set; }
    }
}
