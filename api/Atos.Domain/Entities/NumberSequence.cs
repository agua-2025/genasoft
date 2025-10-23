using Atos.Domain.Enums;
namespace Atos.Domain.Entities;
public class NumberSequence
{
  public int Id { get; set; }
  public string DocumentTypeKey { get; set; } = "";
  public int? Year { get; set; }              // null = sequência contínua
  public int? OrganId { get; set; }           // futuro: sequência por órgão
  public int CurrentNumber { get; set; }
  public string Mask { get; set; } = "{num}/{year}";
  public NumberingStrategy Strategy { get; set; } = NumberingStrategy.Yearly;
}
