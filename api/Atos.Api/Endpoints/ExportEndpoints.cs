using System.Security.Claims;
using System.Linq;
using Atos.Application.Services;
using Atos.Application.Services.Export;
using Atos.Infrastructure.Data;

namespace Atos.Api.Endpoints;

public static class ExportEndpoints
{
  public static void MapExportEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapPost("/api/export/transparencia/{id:long}", async (
      long id,
      TransparencyExportService exportService,
      ClaimsPrincipal user,
      AtosDbContext db,
      CancellationToken ct) =>
    {
      try
      {
        var result = await exportService.ExportAsync(id, ct);
        await AuditLogger.LogAsync(db, id, "export", GetUserId(user), result.BlobUrl, ct);
        return Results.Ok(new { result.BlobName, result.BlobUrl });
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
    .WithName("ExportTransparency")
    .RequireAuthorization(policy =>
      policy.RequireAssertion(ctx =>
        ctx.User?.FindAll("roles")
           .Any(c => string.Equals(c.Value, "Admin", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(c.Value, "Redator", StringComparison.OrdinalIgnoreCase)) ?? false));
  }

  private static string GetUserId(ClaimsPrincipal user) =>
    user.FindFirst("oid")?.Value
    ?? user.FindFirst("preferred_username")?.Value
    ?? user.Identity?.Name
    ?? "unknown";
}
