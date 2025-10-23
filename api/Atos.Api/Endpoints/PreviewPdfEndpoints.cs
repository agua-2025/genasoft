using System.Security.Claims;
using System.Linq;
using Atos.Application.DTOs;
using Atos.Application.Services;
using Atos.Application.Services.Pdf;
using Atos.Infrastructure.Data;

namespace Atos.Api.Endpoints;

public static class PreviewPdfEndpoints
{
  public static void MapPreviewPdfEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapPost("/api/documents/{id:long}/preview-pdf", async (
      long id,
      PreviewPdfRequest request,
      PreviewPdfService service,
      AtosDbContext db,
      ClaimsPrincipal user,
      CancellationToken ct) =>
    {
      try
      {
        var result = await service.GenerateAsync(id, request, ct);
        await AuditLogger.LogAsync(db, id, "preview-pdf", GetUserId(user), result.BlobUrl, ct);
        return Results.Ok(new { result.BlobName, result.BlobUrl, result.Sha256 });
      }
      catch (KeyNotFoundException)
      {
        return Results.NotFound();
      }
      catch (InvalidOperationException ex)
      {
        return Results.BadRequest(new { message = ex.Message });
      }
    })
    .WithName("PreviewPdf")
    .RequireAuthorization(policy =>
      policy.RequireAssertion(ctx =>
        ctx.User?.FindAll("roles")
           .Any(c => string.Equals(c.Value, "Admin", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(c.Value, "Redator", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(c.Value, "Autoridade", StringComparison.OrdinalIgnoreCase)) ?? false));
  }

  private static string GetUserId(ClaimsPrincipal user) =>
    user.FindFirst("oid")?.Value
    ?? user.FindFirst("preferred_username")?.Value
    ?? user.Identity?.Name
    ?? "unknown";
}
