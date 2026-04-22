using Microsoft.EntityFrameworkCore;
using ClubBAIST.Models.Domain;

namespace ClubBAIST.Data
{
    public class ClubBAISTContext : DbContext
    {
        public ClubBAISTContext(DbContextOptions<ClubBAISTContext> options)
            : base(options)
        {
        }

        // Membership
        public DbSet<MembershipCategory> MembershipCategories { get; set; }
        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<MembershipApplication> MembershipApplications { get; set; }

        // Financial
        public DbSet<MemberAccount> MemberAccounts { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }

        // Tee Times
        public DbSet<TeeSheet> TeeSheets { get; set; }
        public DbSet<TeeTimeSlot> TeeTimeSlots { get; set; }
        public DbSet<TeeTimeReservation> TeeTimeReservations { get; set; }
        public DbSet<ReservationPlayer> ReservationPlayers { get; set; }
        public DbSet<StandingTeeTimeRequest> StandingTeeTimeRequests { get; set; }

        // Golf & Scoring
        public DbSet<GolfCourse> GolfCourses { get; set; }
        public DbSet<CourseTee> CourseTees { get; set; }
        public DbSet<CourseHole> CourseHoles { get; set; }
        public DbSet<GolfRound> GolfRounds { get; set; }
        public DbSet<RoundHoleScore> RoundHoleScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascading delete issues on self-referencing foreign keys
            modelBuilder.Entity<MembershipApplication>()
                .HasOne(a => a.Sponsor1)
                .WithMany()
                .HasForeignKey(a => a.Sponsor1MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MembershipApplication>()
                .HasOne(a => a.Sponsor2)
                .WithMany()
                .HasForeignKey(a => a.Sponsor2MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MembershipApplication>()
                .HasOne(a => a.ReviewedBy)
                .WithMany()
                .HasForeignKey(a => a.ReviewedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeeTimeReservation>()
                .HasOne(r => r.CancelledBy)
                .WithMany()
                .HasForeignKey(r => r.CancelledByMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AccountTransaction>()
                .HasOne(t => t.PostedBy)
                .WithMany()
                .HasForeignKey(t => t.PostedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: one slot per time per tee sheet
            modelBuilder.Entity<TeeTimeSlot>()
                .HasIndex(s => new { s.TeeSheetId, s.SlotTime })
                .IsUnique();

            // Unique constraint: one score per hole per round
            modelBuilder.Entity<RoundHoleScore>()
                .HasIndex(s => new { s.RoundId, s.HoleNumber })
                .IsUnique();
        }
    }
}