using Atos.Domain.Enums;

namespace Atos.Domain.Entities;

public class DocumentRelation
{
    public int Id { get; set; }
    public int SourceDocumentId { get; set; }
    public int TargetDocumentId { get; set; }
    public RelationType RelationType { get; set; }
    public string ScopeJson { get; set; } = null!;
    public string? Notes { get; set; }

    public Document? SourceDocument { get; set; }
    public Document? TargetDocument { get; set; }
}
