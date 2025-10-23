using System.Security.Claims;
using System.Linq;
using Atos.Application.DTOs;
using Atos.Application.Services;
using Atos.Application.Services.Consolidation;
using Atos.Domain.Entities;
using Atos.Domain.Enums;
using Atos.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atos.Api.Endpoints;

public static class DocumentsEndpoints
{
  public static void MapDocumentsEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/documents")
      .RequireAuthorization()
      .RequireRateLimiting("DocsLimiter");

    group.MapPost("/", async (
      CreateDocumentDto dto,
      AtosDbContext db,
      HtmlSanitizerService sanitizer,
      ClaimsPrincipal user,
      CancellationToken ct) =>
    {
      var doc = new Document
      {
        TypeKey = dto.TypeKey?.Trim() ?? "",
        OrganId = dto.OrganId,
        Title = dto.Title?.Trim() ?? "",
        Subject = dto.Subject,
        TextHtml = sanitizer.Sanitize(dto.TextHtml),
        Year = dto.Year ?? DateTime.UtcNow.Year,
        Status = DocumentStatus.Draft
      };

      db.Documents.Add(doc);
      await db.SaveChangesAsync(ct);
      await AuditLogger.LogAsync(db, doc.Id, "create", GetUserId(user), $"Created document {doc.Title}", ct);

      return Results.Created($"/api/documents/{doc.Id}", new { doc.Id });
    }).RequireRoles("Admin", "Redator");

    group.MapPut("/{id:long}", async (
      long id,
      UpdateDocumentDto dto,
      AtosDbContext db,
      HtmlSanitizerService sanitizer,
      ClaimsPrincipal user,
      CancellationToken ct) =>
    {
      var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
      if (doc is null)
        return Results.NotFound();

      if (doc.Status is not DocumentStatus.Draft and not DocumentStatus.InReview)
        return Results.BadRequest(new { message = "Document cannot be edited in current status." });

      doc.Title = dto.Title?.Trim() ?? doc.Title;
      doc.Subject = dto.Subject;
      doc.TextHtml = sanitizer.Sanitize(dto.TextHtml);

      await db.SaveChangesAsync(ct);
      await AuditLogger.LogAsync(db, doc.Id, "edit", GetUserId(user), $"Edited document {doc.Title}", ct);

      return Results.Ok(new { doc.Id, status = doc.Status.ToString() });
    }).RequireRoles("Admin", "Redator");

    group.MapPost("/{id:long}/approve", async (
      long id,
      ApproveDto dto,
      AtosDbContext db,
      ClaimsPrincipal user,
      CancellationToken ct) =>
    {
      var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
      if (doc is null)
        return Results.NotFound();

      if (doc.Status is not DocumentStatus.Draft and not DocumentStatus.InReview)
        return Results.BadRequest(new { message = "Only draft or in-review documents can be approved." });

      doc.Status = DocumentStatus.Approved;

      await db.SaveChangesAsync(ct);
      await AuditLogger.LogAsync(db, doc.Id, "approve", GetUserId(user), dto.Notes, ct);

      return Results.Ok(new { doc.Id, status = doc.Status.ToString() });
    }).RequireRoles("Admin", "Redator");

    group.MapPost("/{id:long}/allocate-number", async (
      long id,
      AtosDbContext db,
      AllocateNumberService allocator,
      ClaimsPrincipal user,
      CancellationToken ct) =>
    {
      var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
      if (doc is null)
        return Results.NotFound();

      if (doc.Status is not DocumentStatus.Approved)
        return Results.BadRequest(new { message = "Only approved documents can be numbered." });

      var allocation = await allocator.AllocateForDocumentAsync(doc, ct);

      doc.Number = allocation.number;
      doc.Year = allocation.year;
      doc.Status = DocumentStatus.Numbered;

      await db.SaveChangesAsync(ct);
      await AuditLogger.LogAsync(db, doc.Id, "allocate-number", GetUserId(user), allocation.formatted, ct);

      return Results.Ok(new
      {
        doc.Id,
        number = allocation.number,
        year = allocation.year,
        formatted = allocation.formatted
      });
    }).RequireRoles("Admin", "Redator");

    group.MapPost("/{id:long}/sign", async (
      long id,
      SignDto dto,
      AtosDbContext db,
      ClaimsPrincipal user,
      CancellationToken ct) =>
    {
      var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
      if (doc is null)
        return Results.NotFound();

      if (doc.Status is not DocumentStatus.Numbered)
        return Results.BadRequest(new { message = "Only numbered documents can be signed." });

      doc.Status = DocumentStatus.Signed;
      doc.HashSha256 = Guid.NewGuid().ToString("N");

      await db.SaveChangesAsync(ct);
      var details = string.IsNullOrWhiteSpace(dto.SignerName) ? "Document signed." : $"Document signed by {dto.SignerName}.";
      await AuditLogger.LogAsync(db, doc.Id, "sign", GetUserId(user), details, ct);

      return Results.Ok(new { doc.Id, status = doc.Status.ToString(), hash = doc.HashSha256 });
    }).RequireRoles("Autoridade");

    group.MapPost("/{id:long}/publish", async (
      long id,
      PublishDto dto,
      AtosDbContext db,
      ClaimsPrincipal user,
      CancellationToken ct) =>
    {
      var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
      if (doc is null)
        return Results.NotFound();

      if (doc.Status is not DocumentStatus.Signed and not DocumentStatus.Numbered)
        return Results.BadRequest(new { message = "Only signed or numbered documents can be published." });

      doc.Status = DocumentStatus.Published;
      var publishedAt = dto.PublishedAt?.UtcDateTime ?? DateTime.UtcNow;
      doc.PublishedAt = publishedAt;

      await db.SaveChangesAsync(ct);
      await AuditLogger.LogAsync(db, doc.Id, "publish", GetUserId(user), $"Published at {publishedAt:u}", ct);

      return Results.Ok(new { doc.Id, status = doc.Status.ToString(), doc.PublishedAt });
    }).RequireRoles("Admin", "Redator");

    group.MapGet("/", async (
      [AsParameters] FilterDocumentsDto filter,
      AtosDbContext db,
      CancellationToken ct) =>
    {
      var query = db.Documents.AsNoTracking().AsQueryable();

      if (!string.IsNullOrWhiteSpace(filter.TypeKey))
        query = query.Where(d => d.TypeKey == filter.TypeKey);

      if (filter.Year is not null)
        query = query.Where(d => d.Year == filter.Year);

      if (!string.IsNullOrWhiteSpace(filter.Status) &&
          Enum.TryParse<DocumentStatus>(filter.Status, true, out var status))
      {
        query = query.Where(d => d.Status == status);
      }

      var list = await query
        .OrderByDescending(d => d.Year)
        .ThenBy(d => d.Number ?? int.MaxValue)
        .ThenByDescending(d => d.Id)
        .Select(d => new
        {
          d.Id,
          d.TypeKey,
          d.Year,
          d.Number,
          d.Title,
          d.Status
        })
        .ToListAsync(ct);

      return Results.Ok(list.Select(d => new
      {
        d.Id,
        d.TypeKey,
        d.Year,
        d.Number,
        d.Title,
        Status = d.Status.ToString()
      }));
    });

    group.MapGet("/{id:long}/consolidated-preview", async (
      long id,
      ConsolidatedPreviewService service,
      CancellationToken ct) =>
    {
      try
      {
        var preview = await service.GenerateAsync(id, ct);
        return Results.Ok(preview);
      }
      catch (InvalidOperationException)
      {
        return Results.NotFound();
      }
    });
  }

  private static RouteHandlerBuilder RequireRoles(this RouteHandlerBuilder builder, params string[] roles) =>
    builder.RequireAuthorization(policy =>
      policy.RequireAssertion(ctx =>
        ctx.User?.FindAll("roles")
           .Any(c => roles.Contains(c.Value, StringComparer.OrdinalIgnoreCase)) ?? false));

  private static string GetUserId(ClaimsPrincipal user) =>
    user.FindFirst("oid")?.Value
    ?? user.FindFirst("preferred_username")?.Value
    ?? user.Identity?.Name
    ?? "unknown";
}
