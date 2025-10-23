using Atos.Domain.Entities;
using Atos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Atos.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AtosDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.DocumentTypes.AnyAsync())
        {
            db.DocumentTypes.AddRange(
                new DocumentType { Key = "decreto", Name = "Decreto" },
                new DocumentType { Key = "portaria", Name = "Portaria" },
                new DocumentType { Key = "instrucao_normativa", Name = "Instrução Normativa" }
            );
        }

        if (!await db.Organs.AnyAsync())
        {
            db.Organs.AddRange(
                new Organ { Name = "Gabinete do Prefeito", Acronym = "GP" },
                new Organ { Name = "Secretaria Municipal de Saúde", Acronym = "SMS" }
            );
        }

        // Sequências: decreto = contínua; portaria/IN = anual (começa no ano atual)
        var year = DateTime.UtcNow.Year;

        if (!await db.NumberSequences.AnyAsync())
        {
            db.NumberSequences.AddRange(
                new NumberSequence {
                    DocumentTypeKey = "decreto",
                    Year = null,
                    OrganId = null,
                    CurrentNumber = 0,
                    Strategy = NumberingStrategy.Continuous,
                    Mask = "DECRETO Nº {num}"
                },
                new NumberSequence {
                    DocumentTypeKey = "portaria",
                    Year = year,
                    OrganId = null,
                    CurrentNumber = 0,
                    Strategy = NumberingStrategy.Yearly,
                    Mask = "PORTARIA Nº {num}/{year}"
                },
                new NumberSequence {
                    DocumentTypeKey = "instrucao_normativa",
                    Year = year,
                    OrganId = null,
                    CurrentNumber = 0,
                    Strategy = NumberingStrategy.Yearly,
                    Mask = "IN Nº {num}/{year}"
                }
            );
        }

        await db.SaveChangesAsync();
    }
}
