using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JconsultGC.Models;

namespace JconsultGC.Data;

public class ProjetIdDbContext : IdentityDbContext<User>
{
    public ProjetIdDbContext(DbContextOptions<ProjetIdDbContext> options) : base(options)
    {
    }

    public DbSet<LoginLog> LoginLogs { get; set; } = null!;
    public DbSet<ActiveSession> ActiveSessions { get; set; } = null!;
    public DbSet<Account>? Accounts { get; set; }
    public DbSet<Courrier> Courriers { get; set; } = null!;
    public DbSet<Correspondant>? Correspondants { get; set; }
    public DbSet<Document>? Documents { get; set; }
    public DbSet<TypeDossier>? TypeDossiers { get; set; }
    public DbSet<DossierClassement>? DossierClassements { get; set; }
    public DbSet<CategorieCourrier>? CategorieCourriers { get; set; }
    public DbSet<NatureCourrier>? NatureCourriers { get; set; }
    public DbSet<ModeEnvoi>? ModeEnvois { get; set; }
    public DbSet<CircuitCourrier>? CircuitCourriers { get; set; }
    public DbSet<CircuitEtape>? CircuitEtapes { get; set; }
    public DbSet<Signature>? Signatures { get; set; }
    public DbSet<Service>? Services { get; set; }
    public DbSet<Annotation>? Annotations { get; set; }
    public DbSet<Rapport>? Rapports { get; set; }
    public DbSet<ParametreSociete>? ParametresSociete { get; set; }
    public DbSet<Email>? Emails { get; set; }
    public DbSet<CourrierHistory>? CourrierHistories { get; set; }
    public DbSet<ModificationCourrier>? ModificationsCourrier { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Account precision and relationship to User (IdentityUser.Id is string)
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(a => a.Balance).HasPrecision(18, 2);
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Accounts)
                  .HasForeignKey(a => a.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);
        });

      modelBuilder.Entity<Courrier>(entity =>
      {
        entity.HasOne(c => c.CreatedBy)
            .WithMany()
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);
      });

      modelBuilder.Entity<CourrierHistory>(entity =>
      {
        entity.HasOne(h => h.Courrier)
            .WithMany()
            .HasForeignKey(h => h.CourrierId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.SetNull);
      });
    }
}
