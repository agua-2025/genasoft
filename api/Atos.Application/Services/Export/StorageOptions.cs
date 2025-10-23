namespace Atos.Application.Services.Export;

public class StorageOptions
{
  public string BlobServiceUri { get; set; } = string.Empty;
  public string ContainerExport { get; set; } = "export";
  public string ContainerPreview { get; set; } = "previews";
  public bool UseManagedIdentity { get; set; } = true;
}
