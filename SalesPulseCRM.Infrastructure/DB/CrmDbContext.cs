using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Domain.Entities;

namespace SalesPulseCRM.Infrastructure.DB;

public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Lead> Leads { get; set; }
    public DbSet<LeadSource> LeadSources { get; set; }
    public DbSet<LeadStatus> LeadStatuses { get; set; }

    public DbSet<LeadAssignment> LeadAssignments { get; set; }
    public DbSet<LeadNote> LeadNotes { get; set; }
    public DbSet<Followup> Followups { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<EmailQueue> EmailQueues { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<State> States { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Project> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 🔹 LEADS
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.LeadId);

            entity.Property(e => e.CustomerName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(150);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.LastUpdatedDate);

            // 🔗 LeadSource FK
            entity.HasOne(e => e.LeadSource)
                .WithMany(s => s.Leads)
                .HasForeignKey(e => e.LeadSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔗 LeadStatus FK
            entity.HasOne(e => e.LeadStatus)
                .WithMany(s => s.Leads)
                .HasForeignKey(e => e.LeadStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 🔹 LEAD SOURCE
        modelBuilder.Entity<LeadSource>(entity =>
        {
            entity.HasKey(e => e.LeadSourceId);
            entity.Property(e => e.SourceName).HasMaxLength(100).IsRequired();
        });

        // 🔹 LEAD STATUS
        modelBuilder.Entity<LeadStatus>(entity =>
        {
            entity.HasKey(e => e.LeadStatusId);
            entity.Property(e => e.StatusName).HasMaxLength(50).IsRequired();
        });

        // 🔹 LEAD ASSIGNMENT
        modelBuilder.Entity<LeadAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId);

            entity.Property(e => e.AssignedDate)
                .HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Assignments)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 🔹 LEAD NOTES
        modelBuilder.Entity<LeadNote>(entity =>
        {
            entity.HasKey(e => e.NoteId);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Notes)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 🔹 FOLLOWUPS
        modelBuilder.Entity<Followup>(entity =>
        {
            entity.HasKey(e => e.FollowupId);

            entity.Property(e => e.FollowupDateTime).IsRequired();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(e => e.Lead)
                .WithMany(l => l.Followups)
                .HasForeignKey(e => e.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 🔹 NOTIFICATIONS
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);

            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(500);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");
        });

        // 🔹 EMAIL QUEUE
        modelBuilder.Entity<EmailQueue>(entity =>
        {
            entity.HasKey(e => e.EmailId);

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");
        });

        // 🔹 USERS
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETDATE()");
        });
    }
}