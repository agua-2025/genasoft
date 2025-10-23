using Atos.Domain.Enums;

namespace Atos.Domain.Entities;

public class NumberSequence
{
    public int Id { get; set; }
    public string DocumentTypeKey { get; set; } = null!;
    public int? Year { get; set; }
    public int? OrganId { get; set; }
    public int CurrentNumber { get; set; }
    public string Mask { get; set; } = null!;
    public NumberingStrategy Strategy { get; set; }

    public DocumentType? DocumentType { get; set; }
    public Organ? Organ { get; set; }
}
