using System.Text.Json.Serialization;
using Atos.Application.Services;
using Atos.Domain.Entities;
using Atos.Domain.Enums;
using Atos.Infrastructure.Data;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;

namespace Atos.Api.Endpoints;

public static class DocumentsEndpoints
{
    public static RouteGroupBuilder MapDocuments(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents");

        group.MapPost("/", async (AtosDbContext db, HtmlSanitizer sanitizer, CreateDoc dto) =>
        {
            var doc = new Document
            {
                TypeKey = dto.TypeKey,
                Year = dto.Year,
                Title = dto.Title,
                Subject = dto.Subject,
                TextHtml = sanitizer.Sanitize(dto.TextHtml),
                Status = DocumentStatus.Draft
            };
            db.Documents.Add(doc);
            await db.SaveChangesAsync();
            return Results.Created($"/api/documents/{doc.Id}", new { doc.Id });
        });

        group.MapPost("/{id:int}/approve", async (int id, AtosDbContext db) =>
        {
            var doc = await db.Documents.FindAsync(id);
            if (doc is null) return Results.NotFound();
            doc.Status = DocumentStatus.Approved;
            await db.SaveChangesAsync();
            return Results.Ok(new { id, status = doc.Status.ToString() });
        });

        group.MapPost("/{id:int}/allocate-number", async (int id, AtosDbContext db, AllocateNumberService svc) =>
        {
            var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (doc is null) return Results.NotFound();

            var alloc = await svc.AllocateAsync(doc);
            await db.SaveChangesAsync();

            return Results.Ok(new { id, number = alloc.Number, formatted = alloc.Formatted, year = alloc.Year });
        });

        group.MapGet("/", async (AtosDbContext db, string? typeKey, int? year, string? status) =>
        {
            var q = db.Documents.AsQueryable();
            if (!string.IsNullOrWhiteSpace(typeKey)) q = q.Where(d => d.TypeKey == typeKey);
            if (year is not null) q = q.Where(d => d.Year == year);
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DocumentStatus>(status, true, out var st))
                q = q.Where(d => d.Status == st);

            var list = await q.OrderByDescending(d => d.Id)
                .Select(d => new { d.Id, d.TypeKey, d.Year, d.Number, d.Title, Status = d.Status.ToString() })
                .ToListAsync();

            return Results.Ok(list);
        });

        return group;
    }

    public record CreateDoc(
        string TypeKey,
        int Year,
        string Title,
        string? Subject,
        [property: JsonPropertyName("textHtml")] string TextHtml
    );
}
