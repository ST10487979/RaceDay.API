using RaceDay.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
           : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique email constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // One-to-One: User → Organiser
            modelBuilder.Entity<Organiser>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: User → Participant
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Organiser → Events
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many: Event → Categories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique enrolment: (EventID, ParticipantID, CategoryID)
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.EventID, e.ParticipantID, e.CategoryID })
                .IsUnique();

            // One-to-Many: Event → Enrolments
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.NoAction);

            // One-to-Many: Participant → Enrolments
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction);

            // One-to-Many: Category → Enrolments
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);

            // One-to-One: Enrolment → Result
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data (from Part 1)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Users modelBuilder
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, Email = "organiser1@raceday.co.za", PasswordHash = "HASH_ORG1", FullName = "John Mokoena", Role = "Organiser", CreatedAt = new DateTime(2026, 1, 1) },
                new User { UserID = 2, Email = "organiser2@raceday.co.za", PasswordHash = "HASH_ORG2", FullName = "Sarah Naidoo", Role = "Organiser", CreatedAt = new DateTime(2026, 1, 1) },
                new User { UserID = 3, Email = "participant1@raceday.co.za", PasswordHash = "HASH_PART1", FullName = "Thabo Ndlovu", Role = "Participant", CreatedAt = new DateTime(2026, 1, 1) },
                new User { UserID = 4, Email = "participant2@raceday.co.za", PasswordHash = "HASH_PART2", FullName = "Lindiwe Khumalo", Role = "Participant", CreatedAt = new DateTime(2026, 1, 1) }
            );

            // Organisers modelBuilder  
            modelBuilder.Entity<Organiser>().HasData(
                new Organiser { OrganiserID = 1, UserID = 1, CompanyName = "Durban Road Runners Club", ContactPhone = "031-555-0101" },
                new Organiser { OrganiserID = 2, UserID = 2, CompanyName = "Cape Town Cycle Tours", ContactPhone = "021-555-0202" }
            );

            // Participants modelBuilder
            modelBuilder.Entity<Participant>().HasData(
                new Participant { ParticipantID = 1, UserID = 3, DateOfBirth = new DateTime(1990, 5, 15), Province = "Gauteng", EmergencyContact = "Mary Ndlovu - 082-555-1111" },
                new Participant { ParticipantID = 2, UserID = 4, DateOfBirth = new DateTime(1985, 8, 22), Province = "Western Cape", EmergencyContact = "Sipho Khumalo - 083-555-2222" }
            );

            // Events modelBuilder
            modelBuilder.Entity<Event>().HasData(
                new Event { EventID = 1, OrganiserID = 1, Title = "Durban City Marathon 2026", Description = "Annual marathon through Durban", EventDate = new DateTime(2026, 5, 15, 6, 0, 0), Location = "Durban City Hall, KZN", MaxEntries = 5000, Status = "Active", CreatedAt = new DateTime(2026, 1, 1) },
                new Event { EventID = 2, OrganiserID = 1, Title = "Umhlanga 10km Challenge", Description = "Coastal 10km run", EventDate = new DateTime(2026, 6, 20, 7, 0, 0), Location = "Umhlanga Lighthouse, KZN", MaxEntries = 2000, Status = "Active", CreatedAt = new DateTime(2026, 1, 1) },
                new Event { EventID = 3, OrganiserID = 2, Title = "Cape Town Cycle Tour 2026", Description = "109km cycle tour", EventDate = new DateTime(2026, 3, 8, 5, 30, 0), Location = "Cape Town Stadium, WC", MaxEntries = 35000, Status = "Active", CreatedAt = new DateTime(2026, 1, 1) }
            );

            // Categories modelBuilder
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, EventID = 1, Name = "Full Marathon", Distance = 42.20m, EntryFee = 350.00m, MaxCapacity = 1000 },
                new Category { CategoryID = 2, EventID = 1, Name = "Half Marathon", Distance = 21.10m, EntryFee = 250.00m, MaxCapacity = 1500 },
                new Category { CategoryID = 3, EventID = 1, Name = "10km Fun Run", Distance = 10.00m, EntryFee = 150.00m, MaxCapacity = 2500 },
                new Category { CategoryID = 4, EventID = 2, Name = "10km Run", Distance = 10.00m, EntryFee = 100.00m, MaxCapacity = 800 },
                new Category { CategoryID = 5, EventID = 2, Name = "5km Walk", Distance = 5.00m, EntryFee = 50.00m, MaxCapacity = 1200 },
                new Category { CategoryID = 6, EventID = 3, Name = "109km Cycle", Distance = 109.00m, EntryFee = 450.00m, MaxCapacity = 35000 }
            );

            // Enrolments modelBuilder
            modelBuilder.Entity<Enrolment>().HasData(
                new Enrolment { EnrolmentID = 1, EventID = 1, ParticipantID = 1, CategoryID = 1, Status = "Confirmed", PaymentReference = "PAY-2026-001", EntryDate = new DateTime(2026, 1, 15) },
                new Enrolment { EnrolmentID = 2, EventID = 1, ParticipantID = 2, CategoryID = 2, Status = "Confirmed", PaymentReference = "PAY-2026-002", EntryDate = new DateTime(2026, 1, 16) },
                new Enrolment { EnrolmentID = 3, EventID = 2, ParticipantID = 1, CategoryID = 4, Status = "Pending", EntryDate = new DateTime(2026, 1, 20) }
            );

            // Results modelBuilder
            modelBuilder.Entity<Result>().HasData(
                new Result { ResultID = 1, EnrolmentID = 1, FinishTime = new TimeSpan(4, 30, 22), Position = 156, DNF = false, RecordedAt = new DateTime(2026, 5, 15) },
                new Result { ResultID = 2, EnrolmentID = 2, FinishTime = new TimeSpan(2, 15, 45), Position = 89, DNF = false, RecordedAt = new DateTime(2026, 5, 15) }
            );
        }
    }
}