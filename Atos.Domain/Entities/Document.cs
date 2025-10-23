using Atos.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Atos.Domain.Entities;

public class Document
{
    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public string TypeKey { get; set; } = null!;
    public int? OrganId { get; set; }
    public int? Number { get; set; }
    public int Year { get; set; }
    public string Title { get; set; } = null!;
    public string? Subject { get; set; }
    public string TextHtml { get; set; } = null!;
    public DocumentStatus Status { get; set; }
    public DateTime? EffectiveAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? HashSha256 { get; set; }

    public DocumentType? Type { get; set; }
    public Organ? Organ { get; set; }
    public ICollection<DocumentRelation> SourceRelations { get; set; } = new List<DocumentRelation>();
    public ICollection<DocumentRelation> TargetRelations { get; set; } = new List<DocumentRelation>();
}
