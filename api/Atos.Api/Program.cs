using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

using Atos.Application.Services;
using Atos.Application.Services.Consolidation;
using Atos.Application.Services.Export;
using Atos.Application.Services.Pdf;
using Atos.Infrastructure.Data;
using Atos.Api.Endpoints;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Telemetria (pode deixar vazio em DEV)
builder.Services.AddApplicationInsightsTelemetry();

// Key Vault (opcional em DEV; preparado para PROD)
var kvUri = builder.Configuration["Azure:KeyVaultUri"];
if (!string.IsNullOrWhiteSpace(kvUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetTokenBucketLimiter("global", _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            TokensPerPeriod = 100,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        }));

    options.AddTokenBucketLimiter("DocsLimiter", opt =>
    {
        opt.TokenLimit = 30;
        opt.TokensPerPeriod = 30;
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { message = "Too many requests" });
        await context.HttpContext.Response.WriteAsync(payload, token);
    };
});

var corsAllowedOrigins = new[]
{
    "http://localhost:5173",
    "https://web.genasoft.local"
};

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsRestrito", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin)) return false;
            if (corsAllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)) return true;
            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                uri.Host.EndsWith(".azurefd.net", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        });
        policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
        policy.WithHeaders("Authorization", "Content-Type", "Accept");
        policy.DisallowCredentials();
    });
});

// DbContext (SQL local em DEV)
builder.Services.AddDbContext<AtosDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Sql"))
);

// Services
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<PreviewPdfOptions>(builder.Configuration.GetSection("PreviewPdf"));
builder.Services.AddScoped<HtmlSanitizerService>();
builder.Services.AddScoped<AllocateNumberService>();
builder.Services.AddScoped<ConsolidatedPreviewService>();
builder.Services.AddSingleton(provider =>
{
    var storageOptions = provider.GetRequiredService<IOptions<StorageOptions>>().Value;
    var configuration = provider.GetRequiredService<IConfiguration>();

    if (!storageOptions.UseManagedIdentity)
    {
        var connectionString = configuration.GetSection("StorageDev")["ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("StorageDev:ConnectionString must be configured when UseManagedIdentity is false.");
        }

        return new BlobServiceClient(connectionString);
    }

    if (string.IsNullOrWhiteSpace(storageOptions.BlobServiceUri))
    {
        throw new InvalidOperationException("Storage:BlobServiceUri must be configured when UseManagedIdentity is true.");
    }

    var credential = new DefaultAzureCredential();
    return new BlobServiceClient(new Uri(storageOptions.BlobServiceUri), credential);
});
builder.Services.AddScoped<TransparencyExportService>();
builder.Services.AddScoped<PreviewPdfService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Role:Admin", policy => policy.RequireClaim("roles", "Admin"));
    options.AddPolicy("Role:Redator", policy => policy.RequireClaim("roles", "Redator"));
    options.AddPolicy("Role:Autoridade", policy => policy.RequireClaim("roles", "Autoridade"));
});

var app = builder.Build();

// Basic security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["Permissions-Policy"] = "camera=(), geolocation=(), microphone=()";
    if (ctx.Request.IsHttps)
    {
        ctx.Response.Headers["Strict-Transport-Security"] = "max-age=63072000; includeSubDomains; preload";
    }
    await next();
});

app.UseCors("CorsRestrito");
app.UseRateLimiter();

#if DEBUG
// DEV ONLY: injeta um usuário quando os headers X-Dev-* são enviados.
// NÃO usar em Produção.
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Headers.TryGetValue("X-Dev-Auth", out var _))
    {
        var roles = ctx.Request.Headers.TryGetValue("X-Dev-Roles", out var v)
            ? v.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : Array.Empty<string>();

        var claims = new List<System.Security.Claims.Claim> {
            new("name", "dev-user"),
            new("preferred_username", "dev@local"),
        };
        foreach (var r in roles) claims.Add(new("roles", r));

        var id = new System.Security.Claims.ClaimsIdentity(claims, "DevAuth");
        ctx.User = new System.Security.Claims.ClaimsPrincipal(id);
    }
    await next();
});
#endif

app.UseAuthentication();
app.UseAuthorization();

// Health publico
app.MapGet("/health", () => Results.Json(new { status = "ok" })).AllowAnonymous();
app.MapDocumentsEndpoints();
app.MapExportEndpoints();
app.MapPreviewPdfEndpoints();

// Seed DEV (tipos/orgaos/sequencias)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AtosDbContext>();
    await DbInitializer.SeedAsync(db);
}

app.Run();




