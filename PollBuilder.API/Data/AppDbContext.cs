using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // 1. Establish the change-tracking rules for deep collection comparisons
            var stringListComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!), // Determines true element equality order
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Computes stable structural hashing
                c => c.ToList() // Generates deep memory snapshot copies for tracking state
            );

            // 2. Serialize string array seamlessly to a single text field for ease of relational mapping
            modelBuilder.Entity<Poll>()
                .Property(p => p.Options)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>()
                )
                .Metadata.SetValueComparer(stringListComparer); // 🟢 Attaches value comparison telemetry

            modelBuilder.Entity<Vote>()
                .HasIndex(v => new { v.PollCode, v.VoterToken })
                .IsUnique();
        }
    }
}