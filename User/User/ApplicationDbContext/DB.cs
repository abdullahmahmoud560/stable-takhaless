using Microsoft.EntityFrameworkCore;
using User.Model;

namespace User.ApplicationDbContext
{
    public class DB : DbContext
    {
        public DB(DbContextOptions<DB> options):base(options) { }

        public DbSet<NewOrder> newOrders { get; set; }
        public DbSet<UploadFile> uploadFiles { get; set; }
        public DbSet<NumberOfTypeOrder> typeOrders { get; set; }
        public DbSet<Values> value { get; set; }
        public DbSet<NotesCustomerService> notesCustomerServices { get; set; }
        public DbSet<NotesAccounting> notesAccountings { get; set; }
        public DbSet<PaymentDetails> paymentDetails { get; set; }
        public DbSet<SaberCertificates> saberCertificates { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UploadFile>()
              .HasOne(u => u.newOrder)
              .WithMany(u => u.uploadFiles)
              .HasForeignKey(u => u.newOrderId);

           /* modelBuilder.Entity<NumberOfTypeOrder>()
              .HasOne(u => u.newOrder)
              .WithMany(u => u.numberOfTypeOrders)
              .HasForeignKey(u => u.newOrderId);
            */
            modelBuilder.Entity<Values>()
              .HasOne(u => u.newOrder)
              .WithMany(u => u.values)
              .HasForeignKey(u => u.newOrderId);
            
            modelBuilder.Entity<NotesCustomerService>()
              .HasOne(u => u.newOrder)
              .WithMany(u => u.NotesCustomerServices)
              .HasForeignKey(u => u.newOrderId);
            modelBuilder.Entity<NotesAccounting>()
              .HasOne(u => u.newOrder)
              .WithMany(u => u.notesAccountings)
              .HasForeignKey(u => u.newOrderId);
        }
    }
}
