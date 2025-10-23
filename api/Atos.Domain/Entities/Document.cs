using Atos.Domain.Enums;
namespace Atos.Domain.Entities;
public class Document
{
  public int Id { get; set; }
  public Guid Uuid { get; set; } = Guid.NewGuid();
  public string TypeKey { get; set; } = "";   // FK lógica para DocumentType.Key
  public int? OrganId { get; set; }
  public int? Number { get; set; }            // nº do ato (opcional até numerar)
  public int Year { get; set; }               // ano da sequência
  public string Title { get; set; } = "";     // ementa
  public string? Subject { get; set; }
  public string TextHtml { get; set; } = "";  // conteúdo em HTML
  public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
  public DateTime? EffectiveAt { get; set; }
  public DateTime? PublishedAt { get; set; }
  public string? HashSha256 { get; set; }     // após assinatura/publicação
}
