namespace Atos.Application.DTOs;

public class PreviewPdfRequest
{
  public string? Html { get; set; }
  public string? TitleOverride { get; set; }
}

public record PreviewPdfResult(bool Success, string BlobName, string BlobUrl, string Sha256);
