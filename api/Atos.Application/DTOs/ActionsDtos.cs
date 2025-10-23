namespace Atos.Application.DTOs;

public class ApproveDto
{
  public string? Notes { get; set; }
}

public class PublishDto
{
  public DateTimeOffset? PublishedAt { get; set; }
}

public class SignDto
{
  public string? SignerName { get; set; }
}
