using System;
using System.Threading;
using System.Threading.Tasks;
using Atos.Domain.Entities;
using Atos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Atos.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AtosDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        await EnsureDocumentTypesAsync(context, cancellationToken);
        await EnsureOrgansAsync(context, cancellationToken);
        await EnsureNumberSequencesAsync(context, cancellationToken);
    }

    private static async Task EnsureDocumentTypesAsync(AtosDbContext context, CancellationToken cancellationToken)
    {
        var requiredTypes = new[]
        {
            new DocumentType { Key = "decreto", Name = "Decreto" },
            new DocumentType { Key = "portaria", Name = "Portaria" },
            new DocumentType { Key = "instrucao_normativa", Name = "Instrucao Normativa" }
        };

        foreach (var documentType in requiredTypes)
        {
            if (!await context.DocumentTypes.AnyAsync(dt => dt.Key == documentType.Key, cancellationToken))
            {
                context.DocumentTypes.Add(documentType);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureOrgansAsync(AtosDbContext context, CancellationToken cancellationToken)
    {
        var requiredOrgans = new[]
        {
            new Organ { Name = "Gabinete do Prefeito (GP)", Acronym = "GP" },
            new Organ { Name = "Saude (SMS)", Acronym = "SMS" }
        };

        foreach (var organ in requiredOrgans)
        {
            if (!await context.Organs.AnyAsync(o => o.Acronym == organ.Acronym, cancellationToken))
            {
                context.Organs.Add(organ);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureNumberSequencesAsync(AtosDbContext context, CancellationToken cancellationToken)
    {
        var currentYear = DateTime.UtcNow.Year;

        var continuousSequences = new[]
        {
            new NumberSequence
            {
                DocumentTypeKey = "decreto",
                Year = null,
                OrganId = null,
                Mask = "DECRETO Nº {num}",
                Strategy = NumberingStrategy.Continuous,
                CurrentNumber = 0
            }
        };

        foreach (var sequence in continuousSequences)
        {
            if (!await context.NumberSequences.AnyAsync(ns => ns.DocumentTypeKey == sequence.DocumentTypeKey && ns.Year == null && ns.OrganId == sequence.OrganId, cancellationToken))
            {
                context.NumberSequences.Add(sequence);
            }
        }

        var yearlySequences = new[]
        {
            new NumberSequence
            {
                DocumentTypeKey = "portaria",
                Year = currentYear,
                OrganId = null,
                Mask = "PORTARIA Nº {num}/{year}",
                Strategy = NumberingStrategy.Yearly,
                CurrentNumber = 0
            },
            new NumberSequence
            {
                DocumentTypeKey = "instrucao_normativa",
                Year = currentYear,
                OrganId = null,
                Mask = "IN Nº {num}/{year}",
                Strategy = NumberingStrategy.Yearly,
                CurrentNumber = 0
            }
        };

        foreach (var sequence in yearlySequences)
        {
            if (!await context.NumberSequences.AnyAsync(ns => ns.DocumentTypeKey == sequence.DocumentTypeKey && ns.Year == sequence.Year && ns.OrganId == sequence.OrganId, cancellationToken))
            {
                context.NumberSequences.Add(sequence);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
