namespace Atos.Domain.Entities;
public class DocumentType
{
  public int Id { get; set; }
  public string Key { get; set; } = "";   // decreto|portaria|instrucao_normativa
  public string Name { get; set; } = "";
}
