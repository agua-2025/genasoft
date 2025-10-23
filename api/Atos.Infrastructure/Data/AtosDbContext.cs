using Atos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atos.Infrastructure.Data;

public class AtosDbContext : DbContext
{
  public AtosDbContext(DbContextOptions<AtosDbContext> opt) : base(opt) {}

  public DbSet<Organ> Organs => Set<Organ>();
  public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
  public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
  public DbSet<Document> Documents => Set<Document>();
  public DbSet<DocumentRelation> DocumentRelations => Set<DocumentRelation>();
  public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

  protected override void OnModelCreating(ModelBuilder b)
  {
    // === DocumentType ===
    b.Entity<DocumentType>()
      .HasIndex(x => x.Key)
      .IsUnique();

    // === NumberSequence ===
    // Shadow properties computadas para normalizar NULL -> 0
    b.Entity<NumberSequence>()
      .Property<int>("YearNorm")
      .HasComputedColumnSql("ISNULL([Year], 0)", stored: true);

    b.Entity<NumberSequence>()
      .Property<int>("OrganNorm")
      .HasComputedColumnSql("ISNULL([OrganId], 0)", stored: true);

    // Índice ÚNICO na chave lógica: (DocumentTypeKey, YearNorm, OrganNorm)
    b.Entity<NumberSequence>()
      .HasIndex("DocumentTypeKey", "YearNorm", "OrganNorm")
      .IsUnique();

    // === Document ===
    b.Entity<Document>()
      .HasIndex(x => new { x.Year, x.TypeKey, x.Number });

    // enums como string (legibilidade)
    b.Entity<Document>()
      .Property(x => x.Status)
      .HasConversion<string>();

    b.Entity<NumberSequence>()
      .Property(x => x.Strategy)
      .HasConversion<string>();

    b.Entity<AuditEvent>()
      .HasIndex(x => new { x.DocumentId, x.Timestamp });
  }
}
