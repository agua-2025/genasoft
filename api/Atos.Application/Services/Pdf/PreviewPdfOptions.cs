namespace Atos.Application.Services.Pdf;

public class PreviewPdfOptions
{
  public string HeaderTitle { get; set; } = "PREVIEW";
  public bool ShowQr { get; set; } = true;
  public MarginsMm MarginsMm { get; set; } = new();
}

public class MarginsMm
{
  public int Top { get; set; } = 20;
  public int Right { get; set; } = 15;
  public int Bottom { get; set; } = 20;
  public int Left { get; set; } = 15;
}
