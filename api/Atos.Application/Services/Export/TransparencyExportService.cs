using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Atos.Infrastructure.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atos.Application.Services.Export;

public record ExportResult(bool Success, string BlobName, string BlobUrl);

public class TransparencyExportService
{
  private readonly AtosDbContext _db;
  private readonly BlobServiceClient _blobServiceClient;
  private readonly StorageOptions _options;
  private readonly ILogger<TransparencyExportService> _logger;

  public TransparencyExportService(
    AtosDbContext db,
    BlobServiceClient blobSvc,
    IOptions<StorageOptions> opt,
    ILogger<TransparencyExportService> logger)
  {
    _db = db;
    _blobServiceClient = blobSvc;
    _options = opt.Value ?? new StorageOptions();
    _logger = logger;
  }

  public async Task<ExportResult> ExportAsync(long documentId, CancellationToken ct)
  {
    if (documentId > int.MaxValue)
    {
      throw new KeyNotFoundException("Documento não encontrado.");
    }

    var id = (int)documentId;

    var document = await _db.Documents
      .AsNoTracking()
      .FirstOrDefaultAsync(d => d.Id == id, ct);

    if (document is null)
    {
      throw new KeyNotFoundException("Documento não encontrado.");
    }

    var incoming = await _db.DocumentRelations
      .AsNoTracking()
      .Where(r => r.TargetDocumentId == id)
      .ToListAsync(ct);

    var outgoing = await _db.DocumentRelations
      .AsNoTracking()
      .Where(r => r.SourceDocumentId == id)
      .ToListAsync(ct);

    var relatedIds = incoming.Select(r => r.SourceDocumentId)
      .Concat(outgoing.Select(r => r.TargetDocumentId))
      .Distinct()
      .ToList();

    var relatedDocs = await _db.Documents
      .AsNoTracking()
      .Where(d => relatedIds.Contains(d.Id))
      .Select(d => new RelatedDoc
      {
        Id = d.Id,
        TypeKey = d.TypeKey,
        Number = d.Number,
        Year = d.Year,
        Title = d.Title
      })
      .ToDictionaryAsync(d => d.Id, ct);

    var relationsPayload = new List<object>();

    foreach (var relation in incoming)
    {
      relatedDocs.TryGetValue(relation.SourceDocumentId, out var sourceDoc);
      relationsPayload.Add(BuildRelationPayload("entrante", relation, sourceDoc));
    }

    foreach (var relation in outgoing)
    {
      relatedDocs.TryGetValue(relation.TargetDocumentId, out var targetDoc);
      relationsPayload.Add(BuildRelationPayload("sainte", relation, targetDoc));
    }

    var pdfKey = $"{(string.IsNullOrWhiteSpace(document.TypeKey) ? "doc" : document.TypeKey)}_{document.Year}_{(document.Number?.ToString(CultureInfo.InvariantCulture) ?? document.Id.ToString(CultureInfo.InvariantCulture))}";

    var exportPayload = new
    {
      tipo = document.TypeKey,
      numero = document.Number,
      ano = document.Year,
      ementa = document.Title,
      orgaoId = document.OrganId,
      status = document.Status.ToString(),
      datas = new
      {
        effectiveAt = document.EffectiveAt,
        publishedAt = document.PublishedAt
      },
      signatarios = new[]
      {
        new { nome = "Autoridade Mock", cargo = "Prefeito(a)" }
      },
      hash_sha256 = document.HashSha256,
      pdf_url = $"https://portal.mock/pdfs/{pdfKey}.pdf",
      relacoes = relationsPayload
    };

    var json = JsonSerializer.Serialize(exportPayload, new JsonSerializerOptions
    {
      WriteIndented = true,
      DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    });

    var containerName = string.IsNullOrWhiteSpace(_options.ContainerExport)
      ? "export"
      : _options.ContainerExport.ToLowerInvariant();

    var blobName = BuildBlobName(document);

    var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
    await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

    var blobClient = containerClient.GetBlobClient(blobName);
    var bytes = Encoding.UTF8.GetBytes(json);

    using (var stream = new MemoryStream(bytes))
    {
      await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
    }

    await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
    {
      ContentType = "application/json; charset=utf-8"
    }, cancellationToken: ct);

    var blobUrl = blobClient.Uri.ToString();

    _logger.LogInformation("Documento {DocumentId} exportado para blob {BlobName}", documentId, blobName);

    return new ExportResult(true, blobName, blobUrl);
  }

  private static object BuildRelationPayload(string direction, Domain.Entities.DocumentRelation relation, RelatedDoc? relatedDoc)
  {
    object scopeObj;
    if (string.IsNullOrWhiteSpace(relation.ScopeJson))
    {
      scopeObj = new { };
    }
    else
    {
      try
      {
        using var doc = JsonDocument.Parse(relation.ScopeJson);
        scopeObj = doc.RootElement.Clone();
      }
      catch (JsonException)
      {
        scopeObj = new { raw = relation.ScopeJson };
      }
    }

    var relatedInfo = relatedDoc == null
      ? null
      : new
      {
        tipo = relatedDoc.TypeKey,
        numero = relatedDoc.Number,
        ano = relatedDoc.Year,
        titulo = relatedDoc.Title
      };

    return new
    {
      tipo = relation.RelationType.ToString().ToLowerInvariant(),
      direcao = direction,
      doc = relatedInfo,
      escopo = scopeObj
    };
  }

  private static string BuildBlobName(Domain.Entities.Document document)
  {
    var typeSegment = string.IsNullOrWhiteSpace(document.TypeKey)
      ? "documento"
      : document.TypeKey.ToLowerInvariant();

    if (document.Number.HasValue)
    {
      return $"{typeSegment}_{document.Year}_{document.Number.Value}.json";
    }

    return $"{typeSegment}_{document.Id}.json";
  }
  private sealed class RelatedDoc
  {
    public int Id { get; set; }
    public string TypeKey { get; set; } = string.Empty;
    public int? Number { get; set; }
    public int Year { get; set; }
    public string Title { get; set; } = string.Empty;
  }
}
