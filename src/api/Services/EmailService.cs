using System.Text.RegularExpressions;
using System.Threading.Tasks;

using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Task = System.Threading.Tasks.Task;

namespace Vorluno.Contacto.Api.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _opt;
    private readonly ILogger<EmailService> _logger;
    private readonly TransactionalEmailsApi _brevoApi;

    public EmailService(IOptions<EmailOptions> opt, ILogger<EmailService> logger)
    {
        _opt = opt.Value;
        _logger = logger;

        // Configurar SDK de Brevo
        brevo_csharp.Client.Configuration.Default.ApiKey.Clear();
        brevo_csharp.Client.Configuration.Default.ApiKey.Add("api-key", _opt.Brevo.ApiKey);
        brevo_csharp.Client.Configuration.Default.Timeout = _opt.Brevo.TimeoutMs;

        _brevoApi = new TransactionalEmailsApi();
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        string? fromOverride = null,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken ct = default)
    {
        var from = (fromOverride ?? _opt.From)?.Trim();
        var toNorm = to?.Trim();

        if (string.IsNullOrWhiteSpace(from) || !IsValidEmail(from))
            throw new ArgumentException("Remitente inválido (From). Revisa configuración Email:From o fromOverride.");

        if (string.IsNullOrWhiteSpace(toNorm) || !IsValidEmail(toNorm))
            throw new ArgumentException("Destinatario inválido (to).");

        // Construir email de Brevo
        var sendSmtpEmail = new SendSmtpEmail
        {
            Sender = new SendSmtpEmailSender(email: from),
            To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email: toNorm) },
            Subject = subject ?? string.Empty,
            HtmlContent = htmlBody ?? string.Empty,
            TextContent = textBody ?? HtmlToText(htmlBody ?? string.Empty),
            Headers = new Dictionary<string, string>
            {
                { "X-Mailer", "Vorluno.Contacto/1.0" }
            }
        };

        // Agregar attachments si los hay
        if (attachments?.Any() == true)
        {
            sendSmtpEmail.Attachment = attachments.Select(a =>
                new SendSmtpEmailAttachment(
                    content: a.Data,
                    name: a.FileName
                )
            ).ToList();
        }

        await SendWithRetryAsync(sendSmtpEmail, ct);
        _logger.LogInformation("Correo enviado a {To} con asunto {Subject}", toNorm, subject);
    }

    // ----------------- Helpers -----------------

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string HtmlToText(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var withBreaks = Regex.Replace(html, @"<(br|BR)\s*/?>", "\n");
        withBreaks = Regex.Replace(withBreaks, @"</(p|P)\s*>", "\n\n");
        withBreaks = Regex.Replace(withBreaks, @"</(div|DIV)\s*>", "\n");

        var noTags = Regex.Replace(withBreaks, "<.*?>", string.Empty);
        return Regex.Replace(noTags, @"[ \t]+\n", "\n").Trim();
    }

    private async Task SendWithRetryAsync(SendSmtpEmail email, CancellationToken ct)
    {
        const int maxAttempts = 3;
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                // El SDK de Brevo es síncrono, lo envolvemos en Task.Run
                var result = await Task.Run(() => _brevoApi.SendTransacEmail(email), ct);
                _logger.LogInformation("Email enviado exitosamente. MessageId: {MessageId}", result.MessageId);
                return;
            }
            catch (ApiException ex) when (IsTransientError(ex) && attempt < maxAttempts)
            {
                _logger.LogWarning(ex,
                    "Error transitorio de API Brevo ({StatusCode}). Reintentando {Attempt}/{Max}...",
                    ex.ErrorCode, attempt, maxAttempts);
            }
            catch (TaskCanceledException) when (attempt < maxAttempts)
            {
                _logger.LogWarning("Timeout en request a Brevo. Reintentando {Attempt}/{Max}...", attempt, maxAttempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al enviar email después de {Attempt} intentos", attempt);
                throw;
            }

            await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct);
        }
    }

    private static bool IsTransientError(ApiException ex)
    {
        // Errores 5xx del servidor o 429 (rate limit) son transitorios
        return ex.ErrorCode >= 500 || ex.ErrorCode == 429;
    }
}
