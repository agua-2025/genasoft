using System;
using Atos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Atos.Infrastructure.Migrations
{
    [DbContext(typeof(AtosDbContext))]
    partial class AtosDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("Atos.Domain.Entities.Document", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("EffectiveAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("HashSha256")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<int?>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("OrganId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("PublishedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Subject")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("TextHtml")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("TypeKey")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Uuid")
                        .HasColumnType("TEXT");

                    b.Property<int>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OrganId");

                    b.HasIndex("Title");

                    b.HasIndex("TypeKey");

                    b.HasIndex("Year", "TypeKey", "Number");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Atos.Domain.Entities.DocumentRelation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("RelationType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SourceDocumentId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ScopeJson")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("TargetDocumentId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SourceDocumentId");

                    b.HasIndex("TargetDocumentId");

                    b.ToTable("DocumentRelations");
                });

            modelBuilder.Entity("Atos.Domain.Entities.DocumentType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasAlternateKey("Key")
                        .HasName("AK_DocumentTypes_Key");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("DocumentTypes");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Key = "decreto",
                            Name = "Decreto"
                        },
                        new
                        {
                            Id = 2,
                            Key = "portaria",
                            Name = "Portaria"
                        },
                        new
                        {
                            Id = 3,
                            Key = "instrucao_normativa",
                            Name = "Instrucao Normativa"
                        });
                });

            modelBuilder.Entity("Atos.Domain.Entities.NumberSequence", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CurrentNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DocumentTypeKey")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Mask")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int?>("OrganId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Strategy")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DocumentTypeKey");

                    b.HasIndex("DocumentTypeKey", "Year", "OrganId")
                        .IsUnique();

                    b.HasIndex("OrganId");

                    b.ToTable("NumberSequences");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CurrentNumber = 0,
                            DocumentTypeKey = "decreto",
                            Mask = "DECRETO Nº {num}",
                            Strategy = 1,
                            Year = (int?)null
                        },
                        new
                        {
                            Id = 2,
                            CurrentNumber = 0,
                            DocumentTypeKey = "portaria",
                            Mask = "PORTARIA Nº {num}/{year}",
                            Strategy = 2,
                            Year = 2025
                        },
                        new
                        {
                            Id = 3,
                            CurrentNumber = 0,
                            DocumentTypeKey = "instrucao_normativa",
                            Mask = "IN Nº {num}/{year}",
                            Strategy = 2,
                            Year = 2025
                        });
                });

            modelBuilder.Entity("Atos.Domain.Entities.Organ", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Acronym")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParentId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("Organs");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Acronym = "GP",
                            Name = "Gabinete do Prefeito (GP)",
                            ParentId = (int?)null
                        },
                        new
                        {
                            Id = 2,
                            Acronym = "SMS",
                            Name = "Saude (SMS)",
                            ParentId = (int?)null
                        });
                });

            modelBuilder.Entity("Atos.Domain.Entities.Document", b =>
                {
                    b.HasOne("Atos.Domain.Entities.Organ", "Organ")
                        .WithMany("Documents")
                        .HasForeignKey("OrganId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired(false);

                    b.HasOne("Atos.Domain.Entities.DocumentType", "Type")
                        .WithMany("Documents")
                        .HasForeignKey("TypeKey")
                        .HasPrincipalKey("Key")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Organ");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("Atos.Domain.Entities.DocumentRelation", b =>
                {
                    b.HasOne("Atos.Domain.Entities.Document", "SourceDocument")
                        .WithMany("SourceRelations")
                        .HasForeignKey("SourceDocumentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Atos.Domain.Entities.Document", "TargetDocument")
                        .WithMany("TargetRelations")
                        .HasForeignKey("TargetDocumentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("SourceDocument");

                    b.Navigation("TargetDocument");
                });

            modelBuilder.Entity("Atos.Domain.Entities.NumberSequence", b =>
                {
                    b.HasOne("Atos.Domain.Entities.DocumentType", "DocumentType")
                        .WithMany("NumberSequences")
                        .HasForeignKey("DocumentTypeKey")
                        .HasPrincipalKey("Key")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Atos.Domain.Entities.Organ", "Organ")
                        .WithMany("NumberSequences")
                        .HasForeignKey("OrganId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired(false);

                    b.Navigation("DocumentType");

                    b.Navigation("Organ");
                });

            modelBuilder.Entity("Atos.Domain.Entities.Organ", b =>
                {
                    b.HasOne("Atos.Domain.Entities.Organ", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired(false);

                    b.Navigation("Children");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Atos.Domain.Entities.DocumentRelation", b =>
                {
                    b.Navigation("SourceDocument");

                    b.Navigation("TargetDocument");
                });

            modelBuilder.Entity("Atos.Domain.Entities.Document", b =>
                {
                    b.Navigation("SourceRelations");

                    b.Navigation("TargetRelations");
                });

            modelBuilder.Entity("Atos.Domain.Entities.DocumentType", b =>
                {
                    b.Navigation("Documents");

                    b.Navigation("NumberSequences");
                });

            modelBuilder.Entity("Atos.Domain.Entities.NumberSequence", b =>
                {
                    b.Navigation("DocumentType");

                    b.Navigation("Organ");
                });

            modelBuilder.Entity("Atos.Domain.Entities.Organ", b =>
                {
                    b.Navigation("Documents");

                    b.Navigation("NumberSequences");
                });
#pragma warning restore 612, 618
        }
    }
}
