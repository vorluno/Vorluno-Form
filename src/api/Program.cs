using System.IO.Compression;
using Vorluno.Contacto.Api.Services;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON
builder.Services.AddControllers();

// Health checks
builder.Services.AddHealthChecks();

// DI
builder.Services.AddSingleton<IEmailService, EmailService>();

// Email / Google Sheets options
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<GoogleSheetsOptions>(builder.Configuration.GetSection("GoogleSheets"));

// Http logging
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                      HttpLoggingFields.ResponseStatusCode;
});

// Swagger SOLO en desarrollo
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// Compresión (sirve para API y estáticos)
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

// ── CORS ───────────────────────────────────────────────────────
const string DevCors = "DevCors";
const string ProdCors = "ProdCors";
var isDev = builder.Environment.IsDevelopment();

string[] prodOrigins =
    ((builder.Configuration["Cors:ProdOrigin"] ?? Environment.GetEnvironmentVariable("CORS_ORIGIN")) ?? "")
    .Split(new[] { ',', ';', ' ' },
           StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(opt =>
{
    if (isDev)
    {
        opt.AddPolicy(DevCors, p => p
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
    }
    else if (prodOrigins.Length > 0)
    {
        opt.AddPolicy(ProdCors, p => p
            .WithOrigins(prodOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
    }
});

// ── Reverse proxy ─────────────────────────────────────────────
builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opts.KnownNetworks.Clear();
    opts.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cabeceras de seguridad mínimas
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    ctx.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    await next();
});

app.UseRouting();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS (Dev o Prod)
if (isDev) app.UseCors(DevCors);
else if (prodOrigins.Length > 0) app.UseCors(ProdCors);

// Compresión de respuesta (debe ir antes de UseStaticFiles)
app.UseResponseCompression();

// 👉 Static Files (sirve archivos estáticos de wwwroot)
// Configuración de cache: immutable para /assets (Vite genera hash en nombre),
// 10 min para el resto (index.html, etc.)
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var physicalPath = ctx.File.PhysicalPath;
        if (physicalPath != null)
        {
            var p = physicalPath.Replace('\\', '/').ToLowerInvariant();
            if (p.Contains("/assets/"))
            {
                // 1 año + immutable (los nombres cambian por hash)
                ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
            }
            else
            {
                // 10 min para el resto (index.html, etc.)
                ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=600");
            }
        }
    }
});

// Endpoints API / health
app.MapHealthChecks("/healthz");
app.MapControllers();

// SPA fallback
app.MapFallbackToFile("index.html");

app.Run();

public sealed class GoogleSheetsOptions
{
    public string WebhookUrl { get; set; } = "";
    public string? Token { get; set; }
}