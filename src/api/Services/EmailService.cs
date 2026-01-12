using System.Net.Sockets;
using System.Text.RegularExpressions;

using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

namespace Vorluno.Contacto.Api.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _opt;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _env;

    public EmailService(IOptions<EmailOptions> opt, ILogger<EmailService> logger, IWebHostEnvironment env)
    {
        _opt = opt.Value;
        _logger = logger;
        _env = env;
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

        if (string.IsNullOrWhiteSpace(from) || !MailboxAddress.TryParse(from, out var fromAddr))
            throw new ArgumentException("Remitente inválido (From). Revisa configuración Email:From o fromOverride.");

        if (string.IsNullOrWhiteSpace(toNorm) || !MailboxAddress.TryParse(toNorm, out var toAddr))
            throw new ArgumentException("Destinatario inválido (to).");

        var msg = new MimeMessage();
        msg.From.Add(fromAddr);
        msg.To.Add(toAddr);
        msg.Subject = subject ?? string.Empty;
        msg.Headers.Add("X-Mailer", "Vorluno.Contacto/1.0");

        var html = htmlBody ?? string.Empty;
        var fallbackText = HtmlToText(html);

        var builder = new BodyBuilder
        {
            HtmlBody = html,
            TextBody = !string.IsNullOrWhiteSpace(textBody) ? textBody : fallbackText
        };

        // Incrustar solo imágenes inline relevantes (logo, etc.)
        EmbedInlineImages(builder, html);

        if (attachments != null)
        {
            foreach (var a in attachments)
            {
                var parts = (a.ContentType ?? string.Empty).Split('/', 2);
                var mimeType = parts.Length == 2
                    ? new ContentType(parts[0], parts[1])
                    : new ContentType("application", "octet-stream");

                builder.Attachments.Add(a.FileName, a.Data, mimeType);
            }
        }

        msg.Body = builder.ToMessageBody();
        await SendWithRetryAsync(msg, ct);
        _logger.LogInformation("Correo enviado a {To} con asunto {Subject}", toAddr.Address, msg.Subject);
    }

    // ----------------- Helpers -----------------

    private static string HtmlToText(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var withBreaks = Regex.Replace(html, @"<(br|BR)\s*/?>", "\n");
        withBreaks = Regex.Replace(withBreaks, @"</(p|P)\s*>", "\n\n");
        withBreaks = Regex.Replace(withBreaks, @"</(div|DIV)\s*>", "\n");

        var noTags = Regex.Replace(withBreaks, "<.*?>", string.Empty);
        return Regex.Replace(noTags, @"[ \t]+\n", "\n").Trim();
    }

    private void EmbedInlineImages(BodyBuilder builder, string html)
    {
        // Logo inline por CID (vorluno-logo.png)
        TryEmbed(builder, html, _opt.Logo?.File, _opt.Logo?.Cid);

        // El hero del acuse ahora se sirve por URL pública,
        // así que NO lo embebemos como recurso inline.
        // TryEmbed(builder, html, _opt.Ack?.Hero?.File, _opt.Ack?.Hero?.Cid);
    }

    private void TryEmbed(BodyBuilder builder, string html, string? file, string? cid)
    {
        if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(cid))
            return;

        // Solo embebemos si el HTML realmente referencia el CID
        if (!html.Contains($"cid:{cid}", StringComparison.OrdinalIgnoreCase))
            return;

        var path = Path.IsPathRooted(file)
            ? file
            : Path.Combine(_env.ContentRootPath, file);

        if (!System.IO.File.Exists(path))
        {
            _logger.LogWarning("Inline image not found: {Path}", path);
            return;
        }

        var res = builder.LinkedResources.Add(path);
        res.ContentId = cid;
        res.ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Inline);

        if (res is MimePart part)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            part.ContentType.MediaType = "image";
            part.ContentType.MediaSubtype = ext switch
            {
                ".png" => "png",
                ".jpg" or ".jpeg" => "jpeg",
                ".gif" => "gif",
                _ => part.ContentType.MediaSubtype
            };
        }
    }

    private async Task SendWithRetryAsync(MimeMessage msg, CancellationToken ct)
    {
        const int maxAttempts = 3;
        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                using var client = new SmtpClient { Timeout = _opt.Smtp.TimeoutMs };
                var secure = _opt.Smtp.UseStartTls
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(_opt.Smtp.Host, _opt.Smtp.Port, secure, ct).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(_opt.Smtp.User))
                    await client.AuthenticateAsync(_opt.Smtp.User, _opt.Smtp.Password, ct).ConfigureAwait(false);

                await client.SendAsync(msg, ct).ConfigureAwait(false);
                await client.DisconnectAsync(true, ct).ConfigureAwait(false);
                return; // éxito
            }
            catch (SmtpCommandException ex) when (IsTransient(ex.StatusCode) && attempt < maxAttempts)
            {
                _logger.LogWarning(ex,
                    "SMTP transitorio ({Status}). Reintentando {Attempt}/{Max}...",
                    ex.StatusCode, attempt, maxAttempts);
            }
            catch (ServiceNotConnectedException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex,
                    "SMTP no conectado. Reintentando {Attempt}/{Max}...",
                    attempt, maxAttempts);
            }
            catch (SocketException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex,
                    "Error de red. Reintentando {Attempt}/{Max}...",
                    attempt, maxAttempts);
            }
            catch
            {
                throw;
            }

            await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct).ConfigureAwait(false);
        }
    }

    private static bool IsTransient(SmtpStatusCode code) => code is
        SmtpStatusCode.InsufficientStorage or
        SmtpStatusCode.MailboxBusy or
        SmtpStatusCode.MailboxUnavailable or
        SmtpStatusCode.TransactionFailed or
        SmtpStatusCode.ServiceNotAvailable;
}
