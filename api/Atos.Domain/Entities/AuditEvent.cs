namespace Atos.Domain.Entities;
public class AuditEvent
{
  public long Id { get; set; }
  public long DocumentId { get; set; }
  public string Action { get; set; } = "";
  public string UserId { get; set; } = "";
  public DateTimeOffset Timestamp { get; set; }
  public string? Details { get; set; }
}
