using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sciencetopia.Models;

namespace Sciencetopia.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for messaging and user activity
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<VisitLog> VisitLogs { get; set; } // New VisitLog DbSet

        // Add the DailySummaries DbSet
        public DbSet<DailySummary> DailySummaries { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships for the Message model
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional: Configure the Conversation and Notification models if necessary
            // builder.Entity<Conversation>().Has...
            // builder.Entity<Notification>().Has...

            // Configure VisitLog relationships (if needed)
            builder.Entity<VisitLog>()
                .HasKey(v => v.Id); // Primary Key

            builder.Entity<VisitLog>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict); // If visits are linked to users
        }
    }
}
