namespace Atos.Domain.Entities;
public class Organ
{
  public int Id { get; set; }
  public string Name { get; set; } = "";
  public string? Acronym { get; set; }
  public int? ParentId { get; set; }
}
