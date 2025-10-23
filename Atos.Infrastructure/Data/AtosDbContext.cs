using System;
using System.IO;
using Atos.Domain.Entities;
using Atos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Atos.Infrastructure.Data;

public class AtosDbContext : DbContext
{
    public const string DefaultConnectionString = "Data Source=atos.db";

    public AtosDbContext(DbContextOptions<AtosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organ> Organs => Set<Organ>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentRelation> DocumentRelations => Set<DocumentRelation>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var databasePath = Path.Combine(AppContext.BaseDirectory, "atos.db");
            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureOrgan(modelBuilder);
        ConfigureDocumentType(modelBuilder);
        ConfigureNumberSequence(modelBuilder);
        ConfigureDocument(modelBuilder);
        ConfigureDocumentRelation(modelBuilder);
    }

    private static void ConfigureOrgan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organ>(entity =>
        {
            entity.ToTable("Organs");
            entity.Property(o => o.Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(o => o.Acronym)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(o => o.Parent)
                .WithMany(o => o.Children)
                .HasForeignKey(o => o.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new Organ
                {
                    Id = 1,
                    Name = "Gabinete do Prefeito (GP)",
                    Acronym = "GP"
                },
                new Organ
                {
                    Id = 2,
                    Name = "Saude (SMS)",
                    Acronym = "SMS"
                }
            );
        });
    }

    private static void ConfigureDocumentType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("DocumentTypes");
            entity.Property(dt => dt.Key)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(dt => dt.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasAlternateKey(dt => dt.Key)
                .HasName("AK_DocumentTypes_Key");

            entity.HasIndex(dt => dt.Key)
                .IsUnique();

            entity.HasData(
                new DocumentType
                {
                    Id = 1,
                    Key = "decreto",
                    Name = "Decreto"
                },
                new DocumentType
                {
                    Id = 2,
                    Key = "portaria",
                    Name = "Portaria"
                },
                new DocumentType
                {
                    Id = 3,
                    Key = "instrucao_normativa",
                    Name = "Instrucao Normativa"
                }
            );
        });
    }

    private static void ConfigureNumberSequence(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NumberSequence>(entity =>
        {
            entity.ToTable("NumberSequences");
            entity.Property(ns => ns.DocumentTypeKey)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(ns => ns.Mask)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasOne(ns => ns.DocumentType)
                .WithMany(dt => dt.NumberSequences)
                .HasPrincipalKey(dt => dt.Key)
                .HasForeignKey(ns => ns.DocumentTypeKey)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ns => ns.Organ)
                .WithMany(o => o.NumberSequences)
                .HasForeignKey(ns => ns.OrganId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ns => new { ns.DocumentTypeKey, ns.Year, ns.OrganId })
                .IsUnique();

            entity.HasData(
                new NumberSequence
                {
                    Id = 1,
                    DocumentTypeKey = "decreto",
                    Year = null,
                    OrganId = null,
                    CurrentNumber = 0,
                    Mask = "DECRETO Nº {num}",
                    Strategy = NumberingStrategy.Continuous
                },
                new NumberSequence
                {
                    Id = 2,
                    DocumentTypeKey = "portaria",
                    Year = 2025,
                    OrganId = null,
                    CurrentNumber = 0,
                    Mask = "PORTARIA Nº {num}/{year}",
                    Strategy = NumberingStrategy.Yearly
                },
                new NumberSequence
                {
                    Id = 3,
                    DocumentTypeKey = "instrucao_normativa",
                    Year = 2025,
                    OrganId = null,
                    CurrentNumber = 0,
                    Mask = "IN Nº {num}/{year}",
                    Strategy = NumberingStrategy.Yearly
                }
            );
        });
    }

    private static void ConfigureDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.Property(d => d.Uuid)
                .IsRequired();
            entity.Property(d => d.TypeKey)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(d => d.Title)
                .HasMaxLength(500)
                .IsRequired();
            entity.Property(d => d.Subject)
                .HasMaxLength(500);
            entity.Property(d => d.TextHtml)
                .IsRequired();
            entity.Property(d => d.HashSha256)
                .HasMaxLength(64);

            entity.HasOne(d => d.Type)
                .WithMany(dt => dt.Documents)
                .HasPrincipalKey(dt => dt.Key)
                .HasForeignKey(d => d.TypeKey)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Organ)
                .WithMany(o => o.Documents)
                .HasForeignKey(d => d.OrganId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(d => new { d.Year, d.TypeKey, d.Number });
            entity.HasIndex(d => d.Title);
        });
    }

    private static void ConfigureDocumentRelation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentRelation>(entity =>
        {
            entity.ToTable("DocumentRelations");
            entity.Property(dr => dr.ScopeJson)
                .IsRequired();
            entity.Property(dr => dr.Notes)
                .HasMaxLength(500);

            entity.HasOne(dr => dr.SourceDocument)
                .WithMany(d => d.SourceRelations)
                .HasForeignKey(dr => dr.SourceDocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(dr => dr.TargetDocument)
                .WithMany(d => d.TargetRelations)
                .HasForeignKey(dr => dr.TargetDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
