using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PollBuilder.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Poll> Polls => Set<Poll>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<TextQuestion> TextQuestions => Set<TextQuestion>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Serialize string array seamlessly to a single text field for ease of relational mapping
            modelBuilder.Entity<Poll>()
                .Property(p => p.Options)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>()
                );

            modelBuilder.Entity<Vote>()
                .HasIndex(v => new { v.PollCode, v.VoterToken })
                .IsUnique();
        }
    }
}