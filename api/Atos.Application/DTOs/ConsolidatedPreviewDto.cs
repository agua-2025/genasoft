namespace Atos.Application.DTOs;

public class ConsolidatedPreviewDto
{
  public long DocumentId { get; set; }
  public string Title { get; set; } = "";
  public string TypeKey { get; set; } = "";
  public int? Number { get; set; }
  public int Year { get; set; }
  public string Html { get; set; } = "";
  public ConsolidatedMeta Meta { get; set; } = new();
}

public class ConsolidatedMeta
{
  public int IncomingCount { get; set; }
  public int OutgoingCount { get; set; }
  public string GeneratedAtIso { get; set; } = "";
}
