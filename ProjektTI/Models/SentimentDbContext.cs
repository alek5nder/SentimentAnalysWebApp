using Microsoft.EntityFrameworkCore;
using WebAppAI.Models;

namespace WebAppAI.Data
{
    public class SentimentDbContext : DbContext
    {
        public SentimentDbContext(DbContextOptions<SentimentDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SentimentModel> SentimentModels { get; set; }
        public DbSet<MessageAnalysisRecord> MessageAnalyses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UniqueClientId)
                .IsUnique();
        }
    }
}
