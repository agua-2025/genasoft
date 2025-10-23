using System.Data;
using System.Globalization;
using System.Linq;
using Atos.Domain.Entities;
using Atos.Domain.Enums;
using Atos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Atos.Application.Services;

public class AllocateNumberService
{
  private readonly AtosDbContext _db;

  public AllocateNumberService(AtosDbContext db) => _db = db;

  public Task<(int number, int year, string formatted)> AllocateAsync(string typeKey, int? organId, CancellationToken ct) =>
    AllocateInternalAsync(typeKey, organId, null, ct);

  public Task<(int number, int year, string formatted)> AllocateForDocumentAsync(Document doc, CancellationToken ct) =>
    AllocateInternalAsync(doc.TypeKey, doc.OrganId, doc.Year, ct);

  private async Task<(int number, int year, string formatted)> AllocateInternalAsync(string typeKey, int? organId, int? desiredYear, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(typeKey))
      throw new ArgumentException("typeKey is required.", nameof(typeKey));

    var currentYear = DateTime.UtcNow.Year;
    var targetYear = desiredYear ?? currentYear;

    await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

    var sequencesForKey = await _db.NumberSequences
      .Where(s => s.DocumentTypeKey == typeKey)
      .OrderByDescending(s => s.Year)
      .ThenByDescending(s => s.Id)
      .ToListAsync(ct);

    var sequenceCandidates = sequencesForKey
      .Where(s => s.OrganId == organId || (organId == null && s.OrganId == null))
      .ToList();

    var template = sequenceCandidates.FirstOrDefault() ?? sequencesForKey.FirstOrDefault();
    var strategy = template?.Strategy ?? NumberingStrategy.Yearly;
    var mask = template?.Mask ?? GetDefaultMask(typeKey);

    if (strategy == NumberingStrategy.Continuous)
    {
      targetYear = desiredYear ?? template?.Year ?? currentYear;
    }

    if (strategy == NumberingStrategy.YearlyByOrgan && organId is null)
    {
      strategy = NumberingStrategy.Yearly;
    }

    var sequence = sequenceCandidates
      .FirstOrDefault(s => MatchesSequence(s, strategy, organId, strategy == NumberingStrategy.Continuous ? null : targetYear));

    if (sequence is null)
    {
      sequence = new NumberSequence
      {
        DocumentTypeKey = typeKey,
        OrganId = organId,
        Strategy = strategy,
        Year = strategy == NumberingStrategy.Continuous ? null : targetYear,
        CurrentNumber = 0,
        Mask = mask
      };

      _db.NumberSequences.Add(sequence);
    }

    sequence.CurrentNumber += 1;

    await _db.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    var yearForReturn = sequence.Year ?? targetYear;
    var formatted = FormatMask(sequence.Mask, sequence.CurrentNumber, yearForReturn);

    return (sequence.CurrentNumber, yearForReturn, formatted);
  }

  private static bool MatchesSequence(NumberSequence seq, NumberingStrategy strategy, int? organId, int? year)
  {
    if (seq.Strategy != strategy)
      return false;

    if (seq.OrganId != organId)
      return false;

    return strategy switch
    {
      NumberingStrategy.Continuous => seq.Year is null,
      _ => seq.Year == year
    };
  }

  private static string FormatMask(string? mask, int number, int year)
  {
    var pattern = string.IsNullOrWhiteSpace(mask) ? "{num}/{year}" : mask;
    return pattern
      .Replace("{num}", number.ToString(CultureInfo.InvariantCulture))
      .Replace("{year}", year.ToString(CultureInfo.InvariantCulture));
  }

  private static string GetDefaultMask(string typeKey) =>
    typeKey.ToLowerInvariant() switch
    {
      "decreto" => "DECRETO No {num}",
      "portaria" => "PORTARIA No {num}/{year}",
      "instrucao_normativa" => "IN No {num}/{year}",
      _ => "{num}/{year}"
    };
}
