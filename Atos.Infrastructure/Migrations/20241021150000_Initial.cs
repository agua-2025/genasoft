using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atos.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                    table.UniqueConstraint("AK_DocumentTypes_Key", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Organs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Acronym = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organs_Organs_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Organs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NumberSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentTypeKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganId = table.Column<int>(type: "INTEGER", nullable: true),
                    CurrentNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Mask = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Strategy = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberSequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NumberSequences_DocumentTypes_DocumentTypeKey",
                        column: x => x.DocumentTypeKey,
                        principalTable: "DocumentTypes",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NumberSequences_Organs_OrganId",
                        column: x => x.OrganId,
                        principalTable: "Organs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    TypeKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrganId = table.Column<int>(type: "INTEGER", nullable: true),
                    Number = table.Column<int>(type: "INTEGER", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TextHtml = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    EffectiveAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HashSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentTypes_TypeKey",
                        column: x => x.TypeKey,
                        principalTable: "DocumentTypes",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Organs_OrganId",
                        column: x => x.OrganId,
                        principalTable: "Organs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceDocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetDocumentId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelationType = table.Column<int>(type: "INTEGER", nullable: false),
                    ScopeJson = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentRelations_Documents_SourceDocumentId",
                        column: x => x.SourceDocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentRelations_Documents_TargetDocumentId",
                        column: x => x.TargetDocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DocumentTypes",
                columns: new[] { "Id", "Key", "Name" },
                values: new object[,]
                {
                    { 1, "decreto", "Decreto" },
                    { 2, "portaria", "Portaria" },
                    { 3, "instrucao_normativa", "Instrucao Normativa" }
                });

            migrationBuilder.InsertData(
                table: "Organs",
                columns: new[] { "Id", "Acronym", "Name", "ParentId" },
                values: new object[,]
                {
                    { 1, "GP", "Gabinete do Prefeito (GP)", null },
                    { 2, "SMS", "Saude (SMS)", null }
                });

            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CurrentNumber", "DocumentTypeKey", "Mask", "OrganId", "Strategy", "Year" },
                values: new object[,]
                {
                    { 1, 0, "decreto", "DECRETO Nº {num}", null, 1, null },
                    { 2, 0, "portaria", "PORTARIA Nº {num}/{year}", null, 2, 2025 },
                    { 3, 0, "instrucao_normativa", "IN Nº {num}/{year}", null, 2, 2025 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRelations_SourceDocumentId",
                table: "DocumentRelations",
                column: "SourceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRelations_TargetDocumentId",
                table: "DocumentRelations",
                column: "TargetDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OrganId",
                table: "Documents",
                column: "OrganId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Title",
                table: "Documents",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TypeKey",
                table: "Documents",
                column: "TypeKey");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Year_TypeKey_Number",
                table: "Documents",
                columns: new[] { "Year", "TypeKey", "Number" });

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_DocumentTypeKey",
                table: "NumberSequences",
                column: "DocumentTypeKey");

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_OrganId",
                table: "NumberSequences",
                column: "OrganId");

            migrationBuilder.CreateIndex(
                name: "IX_Organs_ParentId",
                table: "Organs",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Key",
                table: "DocumentTypes",
                column: "Key",
                unique: true);

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS UX_NumberSequences_TypeYearOrgan ON NumberSequences (DocumentTypeKey, COALESCE(Year, 0), COALESCE(OrganId, 0));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS UX_NumberSequences_TypeYearOrgan;");

            migrationBuilder.DropTable(
                name: "DocumentRelations");

            migrationBuilder.DropTable(
                name: "NumberSequences");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Organs");
        }
    }
}
