using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

using Atos.Infrastructure.Data;
using Atos.Api.Endpoints;
using Ganss.Xss;

var builder = WebApplication.CreateBuilder(args);

// Telemetria (pode deixar vazio em DEV)
builder.Services.AddApplicationInsightsTelemetry();

// Key Vault (opcional em DEV; preparado para PROD)
var kvUri = builder.Configuration["Azure:KeyVaultUri"];
if (!string.IsNullOrWhiteSpace(kvUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
}

// Rate Limiter (baseline)
builder.Services.AddRateLimiter(_ =>
    _.AddFixedWindowLimiter("default", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 50;
        opt.QueueLimit = 0;
    })
);

// CORS estrito (origens do appsettings.json)
var allowed = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("strict", p => p
        .WithOrigins(allowed)
        .AllowAnyHeader()
        .WithMethods("GET", "POST", "PUT")
    );
});

// DbContext (SQL local em DEV)
builder.Services.AddDbContext<AtosDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Sql"))
);

// ServiÃ§os
builder.Services.AddSingleton(new HtmlSanitizer());
builder.Services.AddScoped<Atos.Application.Services.AllocateNumberService>();

var app = builder.Build();

// Headers de seguranÃ§a bÃ¡sicos
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseCors("strict");
app.UseRateLimiter();

// Health pÃºblico
app.MapGet("/health", () => Results.Json(new { status = "ok" }));

// Endpoints de documentos (criar/aprovar/numerar/listar)
app.MapDocuments();

// Seed DEV (tipos/Ã³rgÃ£os/sequÃªncias)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AtosDbContext>();
    await DbInitializer.SeedAsync(db);
}

app.Run();
