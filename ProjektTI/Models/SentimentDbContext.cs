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
/*

cześć, moaj aplikacja w .net nie zmienia języka poprzez FormLabels, 
    FormLabels samo w sobie działa (wyswietla tekst, ktory okreslilem), ale nie działa przełączananie na FormLabels.en.resx, 
    ktore ma wersje angielska. Kod _Layout: 
*/
