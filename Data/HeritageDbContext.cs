using Microsoft.EntityFrameworkCore;
using DigitalHeritageApp.Models;

namespace DigitalHeritageApp.Data;

public class HeritageDbContext : DbContext
{
    public HeritageDbContext(DbContextOptions<HeritageDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Story> Stories { get; set; } = null!;
    public DbSet<Artifact> Artifacts { get; set; } = null!;
    public DbSet<LanguageEntry> LanguageEntries { get; set; } = null!;
    public DbSet<CulturalEvent> CulturalEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User relationships
        modelBuilder.Entity<User>()
            .HasMany(u => u.Stories)
            .WithOne(s => s.Contributor)
            .HasForeignKey(s => s.ContributorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Artifacts)
            .WithOne(a => a.Contributor)
            .HasForeignKey(a => a.ContributorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasMany(u => u.LanguageEntries)
            .WithOne(l => l.Contributor)
            .HasForeignKey(l => l.ContributorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Events)
            .WithOne(e => e.Contributor)
            .HasForeignKey(e => e.ContributorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed sample user
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Name = "Admin User", 
                Email = "admin@heritage.app",
                Community = "Global",
                Bio = "Heritage preservation facilitator"
            }
        );

        // Seed sample story
        modelBuilder.Entity<Story>().HasData(
            new Story
            {
                Id = 1,
                Title = "The Legend of the Sacred River",
                Description = "An oral tradition passed down through generations about the spiritual significance of the river.",
                Culture = "Indigenous",
                Language = "English",
                Location = "River Valley, Highland Region",
                ContributorId = 1,
                TranscriptionText = "In the beginning, the spirits of our ancestors blessed the river with life-giving water...",
                MediaType = "audio"
            }
        );

        // Seed sample artifact
        modelBuilder.Entity<Artifact>().HasData(
            new Artifact
            {
                Id = 1,
                Name = "Traditional Weaving Loom",
                Description = "A handcrafted wooden loom used for traditional textile weaving.",
                Category = "tools",
                Culture = "Traditional",
                EstimatedAge = "100+ years",
                Materials = "Wood, string",
                Location = "Museum of Cultural Heritage",
                ContributorId = 1
            }
        );

        // Seed sample language entry
        modelBuilder.Entity<LanguageEntry>().HasData(
            new LanguageEntry
            {
                Id = 1,
                Language = "Lakota",
                Word = "Mitakuye Oyasin",
                Translation = "All My Relations",
                PartOfSpeech = "phrase",
                Example = "Mitakuye Oyasin is spoken during prayers to acknowledge connection to all living things.",
                ExampleTranslation = "All my relations in all directions",
                Dialect = "Standing Rock Sioux Tribe",
                ContributorId = 1
            }
        );

        // Seed sample cultural event
        modelBuilder.Entity<CulturalEvent>().HasData(
            new CulturalEvent
            {
                Id = 1,
                Title = "Spring Harvest Festival",
                Description = "Annual celebration of the spring harvest season with traditional music, food, and crafts.",
                Culture = "Traditional",
                Type = "festival",
                StartDate = new DateTime(2026, 06, 21),
                Location = "Central Community Plaza",
                IsRecurring = true,
                RecurrencePattern = "yearly",
                ContributorId = 1,
                Attendees = 500
            }
        );
    }
}
