using System.Collections.Generic;

namespace Atos.Domain.Entities;

public class Organ
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Acronym { get; set; } = null!;
    public int? ParentId { get; set; }
    public Organ? Parent { get; set; }
    public ICollection<Organ> Children { get; set; } = new List<Organ>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<NumberSequence> NumberSequences { get; set; } = new List<NumberSequence>();
}
