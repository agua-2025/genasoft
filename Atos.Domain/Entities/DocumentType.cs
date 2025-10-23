using System.Collections.Generic;

namespace Atos.Domain.Entities;

public class DocumentType
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;
    public string Name { get; set; } = null!;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<NumberSequence> NumberSequences { get; set; } = new List<NumberSequence>();
}
