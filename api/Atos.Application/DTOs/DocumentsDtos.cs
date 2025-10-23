namespace Atos.Application.DTOs;

public class CreateDocumentDto
{
  public string TypeKey { get; set; } = "";
  public int? OrganId { get; set; }
  public string Title { get; set; } = "";
  public string? Subject { get; set; }
  public string TextHtml { get; set; } = "";
  public int? Year { get; set; }
}

public class UpdateDocumentDto
{
  public string Title { get; set; } = "";
  public string? Subject { get; set; }
  public string TextHtml { get; set; } = "";
}

public class FilterDocumentsDto
{
  public string? TypeKey { get; set; }
  public int? Year { get; set; }
  public string? Status { get; set; }
}
