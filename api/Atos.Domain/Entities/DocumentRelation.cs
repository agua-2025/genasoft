using Atos.Domain.Enums;
namespace Atos.Domain.Entities;
public class DocumentRelation
{
  public int Id { get; set; }
  public int SourceDocumentId { get; set; }   // quem altera/revoga
  public int TargetDocumentId { get; set; }   // quem é alterado/revogado
  public RelationType RelationType { get; set; }
  public string ScopeJson { get; set; } = "[]"; // [{"type":"artigo","label":"Art. 1º","anchor":"art-1"}]
  public string? Notes { get; set; }
}
