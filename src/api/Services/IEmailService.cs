namespace Vorluno.Contacto.Api.Services
{
    public record EmailAttachment(string FileName, byte[] Data, string ContentType);

    public interface IEmailService
    {
        Task SendAsync(
            string to,
            string subject,
            string htmlBody,
            string? textBody = null,
            string? fromOverride = null,
            IEnumerable<EmailAttachment>? attachments = null,
            CancellationToken ct = default);
    }
}
