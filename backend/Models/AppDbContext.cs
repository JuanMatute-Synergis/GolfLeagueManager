using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Week> Weeks { get; set; }
        public DbSet<Matchup> Matchups { get; set; }
        public DbSet<PlayerFlightAssignment> PlayerFlightAssignments { get; set; }
        public DbSet<ScoreEntry> ScoreEntries { get; set; }
        public DbSet<HoleScore> HoleScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Player entity
            modelBuilder.Entity<Player>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL UUID generation
            });

            // Configure Season entity
            modelBuilder.Entity<Season>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                    
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                // Configure DateTime properties as UTC
                entity.Property(e => e.StartDate)
                    .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                entity.Property(e => e.EndDate)
                    .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            });

            // Configure Week entity
            modelBuilder.Entity<Week>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                    
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                // Configure DateTime properties as UTC
                entity.Property(e => e.StartDate)
                    .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                entity.Property(e => e.EndDate)
                    .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
                        
                // Configure relationship with Season
                entity.HasOne(w => w.Season)
                    .WithMany(s => s.Weeks)
                    .HasForeignKey(w => w.SeasonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Matchup entity
            modelBuilder.Entity<Matchup>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                    
                // Configure relationship with Week
                entity.HasOne(m => m.Week)
                    .WithMany(w => w.Matchups)
                    .HasForeignKey(m => m.WeekId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Configure relationship with PlayerA
                entity.HasOne(m => m.PlayerA)
                    .WithMany(p => p.MatchupsAsPlayerA)
                    .HasForeignKey(m => m.PlayerAId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                // Configure relationship with PlayerB
                entity.HasOne(m => m.PlayerB)
                    .WithMany(p => p.MatchupsAsPlayerB)
                    .HasForeignKey(m => m.PlayerBId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Flight entity
            modelBuilder.Entity<Flight>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                // Configure relationship with Season (optional)
                entity.HasOne(f => f.Season)
                    .WithMany(s => s.Flights)
                    .HasForeignKey(f => f.SeasonId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.Property(e => e.CreatedAt)
                    .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                entity.Property(e => e.UpdatedAt)
                    .HasConversion(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            });

            // Configure PlayerFlightAssignment entity
            modelBuilder.Entity<PlayerFlightAssignment>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.HasOne(e => e.Player)
                    .WithMany(p => p.FlightAssignments)
                    .HasForeignKey(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Flight)
                    .WithMany()
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // Configure ScoreEntry entity
            modelBuilder.Entity<ScoreEntry>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.HasOne(e => e.Player)
                    .WithMany(p => p.ScoreEntries)
                    .HasForeignKey(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Week)
                    .WithMany(w => w.ScoreEntries)
                    .HasForeignKey(e => e.WeekId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure HoleScore entity
            modelBuilder.Entity<HoleScore>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.HasOne(e => e.Matchup)
                    .WithMany()
                    .HasForeignKey(e => e.MatchupId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Create unique index on MatchupId and HoleNumber
                entity.HasIndex(e => new { e.MatchupId, e.HoleNumber })
                    .IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
