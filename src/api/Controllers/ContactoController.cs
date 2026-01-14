using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vorluno.Contacto.Api.Models;
using Vorluno.Contacto.Api.Services;

namespace Vorluno.Contacto.Api.Controllers;

[ApiController]
[Route("api/contacto")]
[Produces("application/json")]
public sealed class ContactoController : ControllerBase
{
    private readonly IEmailService _email;
    private readonly EmailOptions _opt;
    private readonly ILogger<ContactoController> _log;
    private readonly GoogleSheetsOptions _gs;
    private static readonly HttpClient _http = new HttpClient();

    private const string BRAND = "VORLUNO";
    private const string BRAND_COLOR = "#7C3AED";  // Violet
    private const string ACCENT_COLOR = "#06B6D4";  // Cyan

    public ContactoController(
        IEmailService email,
        IOptions<EmailOptions> opt,
        ILogger<ContactoController> log,
        IOptions<GoogleSheetsOptions> gs)
    {
        _email = email;
        _opt = opt.Value;
        _log = log;
        _gs = gs.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ContactoModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { ok = false, error = "Datos inválidos", errors = ModelState });
        }

        var subject = $"[FORM] Nuevo Lead Vorluno - {model.Nombre}";

        // 1) HTML interno
        var html = BuildInternalHtml(model);
        var text = StripHtml(html);

        // 2) Envío interno (email principal a Vorluno)
        var to = _opt.To ?? "vorluno@gmail.com";
        _log.LogInformation("Enviando correo interno a {To} con asunto: {Subject}", to, subject);

        await _email.SendAsync(
            to: to,
            subject: subject,
            htmlBody: html,
            textBody: text,
            ct: ct
        );

        // 3) Acuse al cliente + Google Sheets EN PARALELO
        var tareas = new List<Task>();

        // Acuse al cliente
        if (_opt.Ack.Enabled && !string.IsNullOrWhiteSpace(model.Email))
        {
            var ackHtml = BuildAckHtml(model.Nombre);
            var ackText = StripHtml(ackHtml);
            var ackSubj = "Gracias por contactar a Vorluno";
            var fromAck = string.IsNullOrWhiteSpace(_opt.Ack.From) ? _opt.From : _opt.Ack.From;

            _log.LogInformation("Enviando acuse a {Correo} con asunto: {Subject}", model.Email, ackSubj);

            tareas.Add(_email.SendAsync(
                to: model.Email,
                subject: ackSubj,
                htmlBody: ackHtml,
                textBody: ackText,
                fromOverride: fromAck,
                ct: ct
            ));
        }

        // Google Sheets (opcional)
        if (!string.IsNullOrWhiteSpace(_gs.WebhookUrl))
        {
            tareas.Add(EnviarAGoogleSheetsAsync(model, ct));
        }

        try
        {
            if (tareas.Count > 0)
            {
                await Task.WhenAll(tareas);
            }
        }
        catch (Exception ex)
        {
            // No rompemos la respuesta porque el correo interno ya salió bien
            _log.LogError(ex, "Error al enviar acuse y/o registrar en Google Sheets.");
        }

        return Ok(new { ok = true });
    }

    // ---- Google Sheets webhook ----
    private async Task EnviarAGoogleSheetsAsync(ContactoModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_gs.WebhookUrl)) return;

        try
        {
            var payload = new
            {
                nombre = model.Nombre,
                email = model.Email,
                telefono = model.Telefono,
                empresa = model.Empresa ?? "",
                tipoProyecto = model.TipoProyecto,
                presupuesto = model.Presupuesto ?? "",
                urgencia = model.Urgencia ?? "",
                mensaje = model.Mensaje,
                fuente = model.Fuente ?? "",
                fechaEnvio = model.FechaEnvio.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.PostAsync(_gs.WebhookUrl, content, ct);
            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("Google Sheets devolvió status {Status}", res.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error al enviar a Google Sheets");
        }
    }

    // ---- Construcción de HTML interno ----
    private string BuildInternalHtml(ContactoModel m)
    {
        var tipoProyectoLabel = GetTipoProyectoLabel(m.TipoProyecto);
        var presupuestoLabel = string.IsNullOrEmpty(m.Presupuesto) ? "No especificado" : GetPresupuestoLabel(m.Presupuesto);
        var urgenciaLabel = string.IsNullOrEmpty(m.Urgencia) ? "No especificada" : GetUrgenciaLabel(m.Urgencia);
        var fuenteLabel = string.IsNullOrEmpty(m.Fuente) ? "No especificada" : GetFuenteLabel(m.Fuente);

        return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <title>Nuevo Lead - {BRAND}</title>
</head>
<body style=""font-family:system-ui,-apple-system,sans-serif;background:#f9fafb;padding:24px 8px;margin:0"">
  <div style=""max-width:640px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.08)"">

    <!-- Header con gradiente -->
    <div style=""background:linear-gradient(135deg, {BRAND_COLOR} 0%, {ACCENT_COLOR} 100%);padding:32px 24px;text-align:center"">
      <div style=""background:#fff;border-radius:12px;padding:16px;display:inline-block"">
        <img src=""{_opt.Logo.Url ?? "https://contacto.vorluno.dev/email-assets/vorluno-logo.png"}"" alt=""{BRAND}"" style=""height:48px;display:block"" />
      </div>
      <h1 style=""color:#fff;margin:16px 0 4px;font-size:24px"">Nuevo Lead</h1>
      <p style=""color:rgba(255,255,255,0.9);margin:0;font-size:14px"">{m.FechaEnvio:yyyy-MM-dd HH:mm:ss} UTC</p>
    </div>

    <!-- Contenido -->
    <div style=""padding:32px 24px"">

      <h2 style=""color:#111827;font-size:18px;margin:0 0 16px"">Información de Contacto</h2>
      <table style=""width:100%;border-collapse:collapse;margin-bottom:24px"">
        <tr>
          <td style=""padding:8px 0;color:#6b7280;font-size:14px;width:40%""><strong>Nombre:</strong></td>
          <td style=""padding:8px 0;color:#111827;font-size:14px"">{m.Nombre}</td>
        </tr>
        <tr>
          <td style=""padding:8px 0;color:#6b7280;font-size:14px""><strong>Email:</strong></td>
          <td style=""padding:8px 0;color:#111827;font-size:14px""><a href=""mailto:{m.Email}"" style=""color:{BRAND_COLOR}"">{m.Email}</a></td>
        </tr>
        <tr>
          <td style=""padding:8px 0;color:#6b7280;font-size:14px""><strong>Teléfono:</strong></td>
          <td style=""padding:8px 0;color:#111827;font-size:14px""><a href=""tel:{m.Telefono}"" style=""color:{BRAND_COLOR}"">{m.Telefono}</a></td>
        </tr>
        {(string.IsNullOrWhiteSpace(m.Empresa) ? "" : $@"
        <tr>
          <td style=""padding:8px 0;color:#6b7280;font-size:14px""><strong>Empresa:</strong></td>
          <td style=""padding:8px 0;color:#111827;font-size:14px"">{m.Empresa}</td>
        </tr>")}
      </table>

      <h2 style=""color:#111827;font-size:18px;margin:24px 0 16px"">Detalles del Proyecto</h2>
      <table style=""width:100%;border-collapse:collapse;margin-bottom:24px"">
        <tr>
          <td style=""padding:8px 0;color:#6b7280;font-size:14px;width:40%""><strong>Tipo de Proyecto:</strong></td>
          <td style=""padding:8px 0;color:#111827;font-size:14px"">{tipoProyectoLabel}</td>
        </tr>
        <tr>
          <td style=""padding:8px 0;color:#6b7280;font-size:14px""><strong>Presupuesto:</strong></td>
          <td style=""padding:8px 0;color:#111827;font-size:14px"">{presupuestoLabel}</td>
        </tr>
        <tr>
          <td style=""padding:8px 0;color:#6b7280;font-size:14px""><strong>Urgencia:</strong></td>
          <td style=""padding:8px 0;color:#111827;font-size:14px"">{urgenciaLabel}</td>
        </tr>
        <tr>
          <td style=""padding:8px 0;color:#6b7280;font-size:14px""><strong>Fuente:</strong></td>
          <td style=""padding:8px 0;color:#111827;font-size:14px"">{fuenteLabel}</td>
        </tr>
      </table>

      <h2 style=""color:#111827;font-size:18px;margin:24px 0 16px"">Mensaje</h2>
      <div style=""background:#f9fafb;padding:16px;border-radius:8px;border-left:4px solid {BRAND_COLOR}"">
        <p style=""margin:0;color:#374151;font-size:14px;line-height:1.6;white-space:pre-wrap"">{m.Mensaje}</p>
      </div>

    </div>

    <!-- Footer -->
    <div style=""background:#f9fafb;padding:24px;text-align:center;border-top:1px solid #e5e7eb"">
      <p style=""margin:0;color:#6b7280;font-size:13px"">
        Este email fue generado automáticamente desde <strong>contacto.vorluno.dev</strong>
      </p>
    </div>
  </div>
</body>
</html>";
    }

    // ---- Construcción de HTML de acuse al cliente ----
    private string BuildAckHtml(string nombre)
    {
        return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
  <title>Gracias por contactarnos - {BRAND}</title>
</head>
<body style=""font-family:system-ui,-apple-system,sans-serif;background:#f9fafb;padding:24px 8px;margin:0"">
  <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.08)"">

    <!-- Hero con gradiente -->
    <div style=""background:linear-gradient(135deg, {BRAND_COLOR} 0%, {ACCENT_COLOR} 100%);padding:48px 24px;text-align:center"">
      <div style=""background:#fff;border-radius:12px;padding:16px;display:inline-block;margin-bottom:24px"">
        <img src=""{_opt.Logo.Url ?? "https://contacto.vorluno.dev/email-assets/vorluno-logo.png"}"" alt=""{BRAND}"" style=""height:48px;display:block"" />
      </div>
      <h1 style=""color:#fff;margin:0 0 12px;font-size:28px"">¡Gracias por contactarnos!</h1>
      <p style=""color:rgba(255,255,255,0.95);margin:0;font-size:16px"">Hemos recibido tu solicitud</p>
    </div>

    <!-- Contenido -->
    <div style=""padding:32px 24px;text-align:center"">
      <p style=""color:#374151;font-size:16px;line-height:1.6;margin:0 0 24px"">
        Hola <strong>{nombre}</strong>,
      </p>
      <p style=""color:#6b7280;font-size:15px;line-height:1.6;margin:0 0 24px"">
        Hemos recibido tu mensaje y nuestro equipo lo revisará pronto.
        Nos pondremos en contacto contigo a la brevedad para discutir tu proyecto.
      </p>

      <div style=""background:#f9fafb;padding:24px;border-radius:8px;margin:24px 0"">
        <p style=""margin:0 0 8px;color:#374151;font-size:14px""><strong>Mientras tanto...</strong></p>
        <p style=""margin:0;color:#6b7280;font-size:14px;line-height:1.6"">
          Puedes conocer más sobre nuestros proyectos y servicios en <a href=""https://vorluno.dev"" style=""color:{BRAND_COLOR};text-decoration:none"">vorluno.dev</a>.
        </p>
      </div>

      <p style=""color:#374151;font-size:15px;margin:32px 0 0"">
        <strong>Equipo Vorluno</strong><br/>
        <span style=""color:#6b7280;font-size:14px"">Transformando ideas en realidad</span>
      </p>
    </div>

    <!-- Footer -->
    <div style=""background:#f9fafb;padding:24px;text-align:center;border-top:1px solid #e5e7eb"">
      <p style=""margin:0 0 8px;color:#6b7280;font-size:13px"">
        © 2026 Vorluno. Todos los derechos reservados.
      </p>
      <p style=""margin:0;color:#9ca3af;font-size:12px"">
        Si no solicitaste este correo, por favor ignóralo.
      </p>
    </div>
  </div>
</body>
</html>";
    }

    // ---- Helpers ----
    private static string StripHtml(string html)
    {
        var sb = new StringBuilder();
        var inTag = false;
        foreach (var c in html)
        {
            if (c == '<') { inTag = true; continue; }
            if (c == '>') { inTag = false; continue; }
            if (!inTag) sb.Append(c);
        }
        return sb.ToString().Trim();
    }

    private static string GetTipoProyectoLabel(string tipo) => tipo?.ToLowerInvariant() switch
    {
        "web" => "Aplicación Web",
        "mobile" => "Aplicación Móvil",
        "ecommerce" => "E-commerce",
        "api" => "API / Backend",
        "consultoria" => "Consultoría",
        "otro" => "Otro",
        _ => tipo ?? "No especificado"
    };

    private static string GetPresupuestoLabel(string presupuesto) => presupuesto?.ToLowerInvariant() switch
    {
        "menos-5k" => "Menos de $5,000",
        "5k-15k" => "$5,000 - $15,000",
        "15k-50k" => "$15,000 - $50,000",
        "mas-50k" => "Más de $50,000",
        _ => presupuesto ?? "No especificado"
    };

    private static string GetUrgenciaLabel(string urgencia) => urgencia?.ToLowerInvariant() switch
    {
        "inmediata" => "Inmediata (necesito empezar ya)",
        "1-2-semanas" => "En 1-2 semanas",
        "1-3-meses" => "En 1-3 meses",
        "explorando" => "Solo explorando opciones",
        _ => urgencia ?? "No especificada"
    };

    private static string GetFuenteLabel(string fuente) => fuente?.ToLowerInvariant() switch
    {
        "google" => "Google",
        "linkedin" => "LinkedIn",
        "referido" => "Referido por alguien",
        "otro" => "Otro",
        _ => fuente ?? "No especificada"
    };
}
