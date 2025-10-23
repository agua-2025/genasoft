using System.IO;
using System.Security.Cryptography;
using System.Text;
using Atos.Application.DTOs;
using Atos.Application.Services.Export;
using Atos.Infrastructure.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace Atos.Application.Services.Pdf;

public class PreviewPdfService
{
  private readonly AtosDbContext _db;
  private readonly BlobServiceClient _blob;
  private readonly StorageOptions _storageOptions;
  private readonly PreviewPdfOptions _pdfOptions;
  private readonly ILogger<PreviewPdfService> _logger;

  public PreviewPdfService(
    AtosDbContext db,
    BlobServiceClient blobSvc,
    IOptions<StorageOptions> storage,
    IOptions<PreviewPdfOptions> pdfOpt,
    ILogger<PreviewPdfService> logger)
  {
    _db = db;
    _blob = blobSvc;
    _storageOptions = storage.Value ?? new StorageOptions();
    _pdfOptions = pdfOpt.Value ?? new PreviewPdfOptions();
    _logger = logger;
  }

  public async Task<PreviewPdfResult> GenerateAsync(long documentId, PreviewPdfRequest req, CancellationToken ct)
  {
    if (documentId > int.MaxValue)
      throw new KeyNotFoundException("Documento nao encontrado.");

    var id = (int)documentId;

    var document = await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
    if (document is null)
      throw new KeyNotFoundException("Documento nao encontrado.");

    var htmlBase = string.IsNullOrWhiteSpace(req.Html) ? document.TextHtml : req.Html;
    if (string.IsNullOrWhiteSpace(htmlBase))
      throw new InvalidOperationException("Documento nao possui conteudo HTML para gerar o preview.");

    var htmlBytes = Encoding.UTF8.GetBytes(htmlBase);
    var hashSha256 = Convert.ToHexString(SHA256.HashData(htmlBytes)).ToLowerInvariant();

    var finalHtml = BuildHtml(document.Title, req.TitleOverride, htmlBase, hashSha256);

    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
      Headless = true
    });
    var context = await browser.NewContextAsync(new BrowserNewContextOptions
    {
      Locale = "pt-BR"
    });
    var page = await context.NewPageAsync();
    await page.SetContentAsync(finalHtml, new PageSetContentOptions { WaitUntil = WaitUntilState.Load });

    var margins = _pdfOptions.MarginsMm ?? new MarginsMm();
    var pdfBytes = await page.PdfAsync(new PagePdfOptions
    {
      Format = "A4",
      Margin = new Margin
      {
        Top = $"{margins.Top}mm",
        Right = $"{margins.Right}mm",
        Bottom = $"{margins.Bottom}mm",
        Left = $"{margins.Left}mm"
      },
      PrintBackground = true
    });

    var containerName = string.IsNullOrWhiteSpace(_storageOptions.ContainerPreview)
      ? "previews"
      : _storageOptions.ContainerPreview.ToLowerInvariant();
    var containerClient = _blob.GetBlobContainerClient(containerName);
    await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

    var blobName = BuildBlobName(document, DateTimeOffset.UtcNow);
    var blobClient = containerClient.GetBlobClient(blobName);

    using (var stream = new MemoryStream(pdfBytes))
    {
      await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
    }

    await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = "application/pdf" }, cancellationToken: ct);

    _logger.LogInformation("Preview PDF gerado para documento {DocumentId} e salvo como {Blob}", documentId, blobName);

    return new PreviewPdfResult(true, blobName, blobClient.Uri.ToString(), hashSha256);
  }

  private string BuildHtml(string originalTitle, string? overrideTitle, string bodyHtml, string hash)
  {
    var title = string.IsNullOrWhiteSpace(overrideTitle) ? originalTitle : overrideTitle;
    var headerTitle = string.IsNullOrWhiteSpace(_pdfOptions.HeaderTitle) ? "PREVIEW" : _pdfOptions.HeaderTitle;

    var sb = new StringBuilder();
    sb.Append("<!doctype html><html lang=\"pt-br\"><head><meta charset=\"utf-8\" />");
    sb.Append("<style>");
    sb.Append("@page { size: A4; margin: 0; } body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; color: #111; }");
    sb.Append(".page { padding: 40px 32px; }");
    sb.Append("header { display: flex; align-items: center; gap: 16px; margin-bottom: 32px; }");
    sb.Append("header img { width: 48px; height: 48px; }");
    sb.Append(".watermark { position: fixed; top: 40%; left: 25%; font-size: 72px; color: rgba(200,200,200,0.25); transform: rotate(-20deg); }");
    sb.Append("h1 { font-size: 20px; margin: 0; }");
    sb.Append(".content { font-size: 13px; line-height: 1.55; }");
    sb.Append("footer { position: fixed; bottom: 24px; left: 32px; right: 32px; font-size: 10px; color: #444; display: flex; justify-content: space-between; align-items: center; }");
    sb.Append(".hash { font-family: 'Courier New', monospace; }");
    sb.Append(".qr { width: 96px; height: 96px; border: 1px solid #888; display: flex; align-items: center; justify-content: center; font-size: 9px; }");
    sb.Append("</style></head><body>");
    sb.Append("<div class='watermark'>PREVIEW</div>");
    sb.Append("<div class='page'>");
    sb.Append("<header>");
    sb.Append("<img src='data:image/svg+xml;utf8," + Uri.EscapeDataString("<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'><circle cx='50' cy='50' r='45' fill='%234a7cff'/><text x='50' y='58' text-anchor='middle' font-size='32' fill='white'>BR</text></svg>") + "' alt='Brasao'/>");
    sb.Append($"<div><div style='text-transform: uppercase; font-size: 12px; letter-spacing: 0.08em;'>{headerTitle}</div><h1>{EscapeHtml(title)}</h1></div>");
    sb.Append("</header>");
    sb.Append($"<div class='content'>{bodyHtml}</div>");
    sb.Append("</div>");

    sb.Append("<footer><div>Hash SHA-256: <span class='hash'>" + hash + "</span></div>");
    if (_pdfOptions.ShowQr)
    {
      var qrSvg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 120 120'><rect width='120' height='120' fill='white'/><text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' font-size='10'>PREVIEW QR</text><text x='50%' y='70%' dominant-baseline='middle' text-anchor='middle' font-size='6'>{hash.Substring(0, 16)}...</text></svg>";
      sb.Append("<div class='qr'><img src='data:image/svg+xml;utf8," + Uri.EscapeDataString(qrSvg) + "' alt='QR Preview'/></div>");
    }
    sb.Append("</footer>");
    sb.Append("</body></html>");
    return sb.ToString();
  }

  private static string EscapeHtml(string? text) =>
    string.IsNullOrEmpty(text) ? string.Empty : System.Net.WebUtility.HtmlEncode(text);

  private static string BuildBlobName(Atos.Domain.Entities.Document document, DateTimeOffset timestamp)
  {
    var typeSegment = string.IsNullOrWhiteSpace(document.TypeKey) ? "documento" : document.TypeKey.ToLowerInvariant();
    var stamp = timestamp.ToString("yyyyMMddHHmmss");
    if (document.Number.HasValue)
      return $"{typeSegment}_{document.Year}_{document.Number.Value}_preview_{stamp}.pdf";
    return $"{typeSegment}_{document.Id}_preview_{stamp}.pdf";
  }
}

