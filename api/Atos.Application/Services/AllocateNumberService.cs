using Atos.Domain.Entities;
using Atos.Domain.Enums;
using Atos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Atos.Application.Services;

public record NumberAllocation(int Number, string Formatted, int? Year);

public class AllocateNumberService
{
    private readonly AtosDbContext _db;
    public AllocateNumberService(AtosDbContext db) => _db = db;

    public async Task<NumberAllocation> AllocateAsync(Document doc, int? organId = null)
    {
        if (doc.Status is not DocumentStatus.Approved and not DocumentStatus.InReview and not DocumentStatus.Draft)
            throw new InvalidOperationException("Documento não está elegível para numeração.");

        int? seqYear = (doc.TypeKey is "portaria" or "instrucao_normativa") ? doc.Year : null;

        using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var seq = await _db.NumberSequences.FirstOrDefaultAsync(s =>
            s.DocumentTypeKey == doc.TypeKey && s.Year == seqYear && s.OrganId == organId);

        if (seq is null)
        {
            seq = new NumberSequence
            {
                DocumentTypeKey = doc.TypeKey,
                Year = seqYear,
                OrganId = organId,
                CurrentNumber = 0,
                Strategy = doc.TypeKey == "decreto" ? NumberingStrategy.Continuous : NumberingStrategy.Yearly,
                Mask = doc.TypeKey switch
                {
                    "decreto" => "DECRETO Nº {num}",
                    "portaria" => "PORTARIA Nº {num}/{year}",
                    "instrucao_normativa" => "IN Nº {num}/{year}",
                    _ => "{num}/{year}"
                }
            };
            _db.NumberSequences.Add(seq);
            await _db.SaveChangesAsync();
        }

        seq.CurrentNumber += 1;
        await _db.SaveChangesAsync();

        await tx.CommitAsync();

        var formatted = seq.Mask
            .Replace("{num}", seq.CurrentNumber.ToString("0"))
            .Replace("{year}", (seq.Year ?? doc.Year).ToString());

        doc.Number = seq.CurrentNumber;
        doc.Status = DocumentStatus.Numbered;

        return new NumberAllocation(seq.CurrentNumber, formatted, seq.Year ?? doc.Year);
    }
}
