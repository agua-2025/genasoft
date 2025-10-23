using Atos.Domain.Entities;
using Atos.Infrastructure.Data;

namespace Atos.Application.Services;

public static class AuditLogger
{
  public static async Task LogAsync(AtosDbContext db, long documentId, string action, string userId, string? details, CancellationToken ct)
  {
    var evt = new AuditEvent
    {
      DocumentId = documentId,
      Action = action,
      UserId = string.IsNullOrWhiteSpace(userId) ? "unknown" : userId,
      Details = string.IsNullOrWhiteSpace(details) ? null : details,
      Timestamp = DateTimeOffset.UtcNow
    };

    db.AuditEvents.Add(evt);
    await db.SaveChangesAsync(ct);
  }
}
