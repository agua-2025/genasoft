using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Atos.Application.DTOs;
using Atos.Domain.Entities;
using Atos.Domain.Enums;
using Atos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Atos.Application.Services.Consolidation;

public class ConsolidatedPreviewService
{
  private readonly AtosDbContext _db;

  public ConsolidatedPreviewService(AtosDbContext db)
  {
    _db = db;
  }

  public async Task<ConsolidatedPreviewDto> GenerateAsync(long documentId, CancellationToken ct)
  {
    if (documentId > int.MaxValue)
      throw new InvalidOperationException("Documento não encontrado.");

    var id = (int)documentId;
    var document = await _db.Documents
      .AsNoTracking()
      .FirstOrDefaultAsync(d => d.Id == id, ct);

    if (document is null)
      throw new InvalidOperationException("Documento não encontrado.");

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
      .Select(d => new RelatedDocument
      {
        Id = d.Id,
        Title = d.Title,
        TypeKey = d.TypeKey,
        Number = d.Number,
        Year = d.Year
      })
      .ToDictionaryAsync(d => d.Id, ct);

    var workingHtml = document.TextHtml ?? string.Empty;
    var topNotices = new List<string>();

    foreach (var relation in incoming)
    {
      relatedDocs.TryGetValue(relation.SourceDocumentId, out var sourceDoc);
      var refText = FormatReference(sourceDoc);
      var scope = ParseScope(relation.ScopeJson);

      switch (relation.RelationType)
      {
        case RelationType.Altera:
          if (scope.Sections.Count == 0)
          {
            topNotices.Add(BuildCallout($"Não foi possível localizar a seção referenciada pela alteração de {refText}."));
            continue;
          }

          var anyAlterChange = false;
          foreach (var sectionId in scope.Sections)
          {
            if (TryTransformSection(ref workingHtml, sectionId, (open, content, close) =>
            {
              anyAlterChange = true;
              var newContent = string.IsNullOrWhiteSpace(scope.NewHtml)
                ? "<em>Nova redação não informada.</em>"
                : scope.NewHtml!;

              var builder = new StringBuilder();
              builder.Append("<div class=\"gs-section-mark\">");
              builder.Append(open);
              builder.Append("<span class=\"gs-del\">");
              builder.Append(content);
              builder.Append("</span>");
              builder.Append(close);
              builder.Append($"<div class=\"gs-note gs-altera\">Redação dada por {refText}</div>");
              builder.Append($"<div class=\"gs-note gs-altera\">{newContent}</div>");
              builder.Append("</div>");
              return builder.ToString();
            }))
            {
              continue;
            }

            topNotices.Add(BuildCallout($"Não foi possível localizar a seção \"{sectionId}\" referida por {refText}."));
          }

          if (!anyAlterChange && scope.Sections.Count == 0 && !string.IsNullOrWhiteSpace(scope.NewHtml))
          {
            topNotices.Add($"<div class=\"gs-note gs-altera\">Redação dada por {refText}: {scope.NewHtml}</div>");
          }
          break;

        case RelationType.Revoga:
          if (scope.Sections.Count == 0)
          {
            topNotices.Add(BuildCallout($"Revogado por {refText}."));
          }
          else
          {
            foreach (var sectionId in scope.Sections)
            {
              if (!TryTransformSection(ref workingHtml, sectionId, (open, content, close) =>
                  {
                    var builder = new StringBuilder();
                    builder.Append("<div class=\"gs-section-mark\">");
                    builder.Append(open);
                    builder.Append("<span class=\"gs-del\">");
                    builder.Append(content);
                    builder.Append("</span>");
                    builder.Append("<span class=\"gs-badge gs-revoga\">");
                    builder.Append($"Revogado por {refText}");
                    builder.Append("</span>");
                    builder.Append(close);
                    builder.Append("</div>");
                    return builder.ToString();
                  }))
              {
                topNotices.Add(BuildCallout($"Não foi possível localizar a seção \"{sectionId}\" referida por {refText}."));
              }
            }
          }
          break;

        case RelationType.Regulamenta:
          topNotices.Add($"<div class=\"gs-callout\">Regulamentado por {refText}</div>");
          break;

        case RelationType.Consolida:
          topNotices.Add($"<div class=\"gs-callout\">Consolidado por {refText}</div>");
          break;
      }
    }

    if (outgoing.Count > 0)
    {
      var builder = new StringBuilder();
      builder.Append("<div class=\"gs-callout\"><strong>Este documento incide sobre:</strong><ul>");
      foreach (var relation in outgoing)
      {
        relatedDocs.TryGetValue(relation.TargetDocumentId, out var targetDoc);
        builder.Append("<li>");
        builder.Append($"{FormatReference(targetDoc)} — {relation.RelationType}");
        builder.Append("</li>");
      }
      builder.Append("</ul></div>");
      topNotices.Add(builder.ToString());
    }

    var cssBlock = "<style>" +
      ".gs-badge{display:inline-block;padding:.125rem .375rem;border-radius:.25rem;font-size:.75rem;border:1px solid #999}" +
      ".gs-revoga{background:#ffecec}" +
      ".gs-altera{background:#ecf7ff}" +
      ".gs-note{margin:.5rem 0;padding:.5rem;border-left:3px solid #999}" +
      ".gs-del{text-decoration:line-through;opacity:.7;display:block}" +
      ".gs-callout{padding:.5rem .75rem;margin:.75rem 0;background:#f6f6f6;border-left:4px solid #777}" +
      ".gs-section-mark{outline:2px dashed #bbb;padding:.25rem;margin:.5rem 0}" +
      "</style>";

    var finalHtml = BuildFinalHtml(workingHtml, cssBlock, topNotices);

    return new ConsolidatedPreviewDto
    {
      DocumentId = documentId,
      Title = document.Title,
      TypeKey = document.TypeKey,
      Number = document.Number,
      Year = document.Year,
      Html = finalHtml,
      Meta = new ConsolidatedMeta
      {
        IncomingCount = incoming.Count,
        OutgoingCount = outgoing.Count,
        GeneratedAtIso = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture)
      }
    };
  }

  private static RelationScope ParseScope(string? scopeJson)
  {
    var scope = new RelationScope();
    if (string.IsNullOrWhiteSpace(scopeJson))
      return scope;

    try
    {
      using var doc = JsonDocument.Parse(scopeJson);
      var root = doc.RootElement;

      if (root.ValueKind == JsonValueKind.Object)
      {
        if (root.TryGetProperty("sections", out var sectionsProp) && sectionsProp.ValueKind == JsonValueKind.Array)
        {
          foreach (var item in sectionsProp.EnumerateArray())
          {
            if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
            {
              scope.Sections.Add(item.GetString()!);
            }
          }
        }

        if (root.TryGetProperty("newHtml", out var newHtmlProp) && newHtmlProp.ValueKind == JsonValueKind.String)
        {
          scope.NewHtml = newHtmlProp.GetString();
        }
      }
      else if (root.ValueKind == JsonValueKind.Array)
      {
        foreach (var item in root.EnumerateArray())
        {
          if (item.ValueKind == JsonValueKind.Object)
          {
            if (item.TryGetProperty("anchor", out var anchorProp) && anchorProp.ValueKind == JsonValueKind.String)
            {
              var anchor = anchorProp.GetString();
              if (!string.IsNullOrWhiteSpace(anchor))
              {
                scope.Sections.Add(anchor!);
              }
            }
          }
          else if (item.ValueKind == JsonValueKind.String)
          {
            var section = item.GetString();
            if (!string.IsNullOrWhiteSpace(section))
            {
              scope.Sections.Add(section!);
            }
          }
        }
      }
    }
    catch (JsonException)
    {
      // Ignored – fallback to empty scope
    }

    return scope;
  }

  private static bool TryTransformSection(ref string html, string sectionId, Func<string, string, string, string> transformer)
  {
    if (string.IsNullOrWhiteSpace(sectionId))
      return false;

    var pattern = $"(<section\\b[^>]*\\bid\\s*=\\s*\"{Regex.Escape(sectionId)}\"[^>]*>)(.*?)(</section>)";
    var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    var match = regex.Match(html);
    if (!match.Success)
      return false;

    var replacement = transformer(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
    html = regex.Replace(html, replacement, 1);
    return true;
  }

  private static string BuildCallout(string content) =>
    $"<div class=\"gs-callout\">{content}</div>";

  private static string FormatReference(RelatedDocument? doc)
  {
    if (doc is null)
      return "documento desconhecido";

    var type = string.IsNullOrWhiteSpace(doc.TypeKey)
      ? "Documento"
      : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(doc.TypeKey.Replace("_", " ").Replace("-", " "));

    if (doc.Number is int number)
    {
      return $"{type} {number}/{doc.Year}";
    }

    return $"{type} {doc.Year}";
  }

  private static string BuildFinalHtml(string originalHtml, string cssBlock, List<string> topNotices)
  {
    var notices = string.Concat(topNotices);

    if (Regex.IsMatch(originalHtml, "<html", RegexOptions.IgnoreCase))
    {
      var withCss = InsertIntoHead(originalHtml, cssBlock);
      return InsertIntoBody(withCss, notices);
    }

    var builder = new StringBuilder();
    builder.Append("<!doctype html><html><head><meta charset=\"utf-8\" />");
    builder.Append(cssBlock);
    builder.Append("</head><body>");
    builder.Append(notices);
    builder.Append(originalHtml);
    builder.Append("</body></html>");
    return builder.ToString();
  }

  private static string InsertIntoHead(string html, string content)
  {
    if (string.IsNullOrEmpty(content))
      return html;

    var closeHead = Regex.Match(html, "</head>", RegexOptions.IgnoreCase);
    if (closeHead.Success)
      return html.Insert(closeHead.Index, content);

    var openHead = Regex.Match(html, "<head[^>]*>", RegexOptions.IgnoreCase);
    if (openHead.Success)
      return html.Insert(openHead.Index + openHead.Length, content);

    return content + html;
  }

  private static string InsertIntoBody(string html, string content)
  {
    if (string.IsNullOrEmpty(content))
      return html;

    var bodyOpen = Regex.Match(html, "<body[^>]*>", RegexOptions.IgnoreCase);
    if (bodyOpen.Success)
      return html.Insert(bodyOpen.Index + bodyOpen.Length, content);

    return content + html;
  }

  private sealed class RelatedDocument
  {
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string TypeKey { get; set; } = "";
    public int? Number { get; set; }
    public int Year { get; set; }
  }

  private sealed class RelationScope
  {
    public List<string> Sections { get; } = new();
    public string? NewHtml { get; set; }
  }
}
