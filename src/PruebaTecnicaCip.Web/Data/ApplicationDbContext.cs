using Microsoft.EntityFrameworkCore;
using PruebaTecnicaCip.Web.Domain;

namespace PruebaTecnicaCip.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<InstitutionalEvent> InstitutionalEvents => Set<InstitutionalEvent>();

    public DbSet<RegistrationRequest> RegistrationRequests => Set<RegistrationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InstitutionalEvent>(entity =>
        {
            entity.ToTable("InstitutionalEvents");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Council)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(e => e.RowVersion)
                .IsRowVersion();

            entity.HasMany(e => e.RegistrationRequests)
                .WithOne(r => r.InstitutionalEvent)
                .HasForeignKey(r => r.InstitutionalEventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(new InstitutionalEvent
            {
                Id = 1,
                Name = "Dia del Padre",
                Council = "Lima",
                Capacity = 2,
                ApprovedCount = 0,
                StartsAt = new DateTime(2026, 6, 15, 9, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });

        modelBuilder.Entity<RegistrationRequest>(entity =>
        {
            entity.ToTable("RegistrationRequests");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Dni)
                .HasMaxLength(8)
                .IsRequired();

            entity.Property(r => r.FullName)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(r => r.Council)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(r => r.ChildDniImagePath)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(r => r.RejectionReason)
                .HasMaxLength(500);

            entity.Property(r => r.RowVersion)
                .IsRowVersion();

            entity.HasIndex(r => new { r.InstitutionalEventId, r.Dni })
                .IsUnique();

            entity.HasIndex(r => r.Status);
        });
    }
}
