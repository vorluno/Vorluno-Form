namespace Vorluno.Contacto.Api.Services;

public sealed class EmailOptions
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public Dictionary<string, string>? Routing { get; set; }

    public BrevoOptions Brevo { get; set; } = new();
    public AckOptions Ack { get; set; } = new();

    // Color de marca + logo para inlines
    public string? BrandColor { get; set; } = "#F47821";
    public LogoOptions? Logo { get; set; } = new();

    public sealed class BrevoOptions
    {
        public string ApiKey { get; set; } = "";
        public int TimeoutMs { get; set; } = 15000;
    }

    public sealed class AckOptions
    {
        public bool Enabled { get; set; } = true;
        public string? From { get; set; }
        public string Subject { get; set; } = "Gracias por contactar a Vorluno";
        public string? HtmlBody { get; set; }
        public string? TextBody { get; set; }
        public ImageOptions? Hero { get; set; } = new();
    }

    public sealed class ImageOptions
    {
        public string? Url { get; set; }  // URL pública para Brevo API
        public string? File { get; set; }  // Mantener para compatibilidad
        public string Cid { get; set; } = "cidImage";
    }

    public sealed class LogoOptions
    {
        public string? Url { get; set; }  // URL pública para Brevo API
        // Ruta relativa al ContentRoot o absoluta
        public string? File { get; set; } = "wwwroot/email-assets/vorluno-logo.png";
        // Content-ID para usar en el HTML: <img src="cid:{Cid}">
        public string Cid { get; set; } = "vorlunoLogo";
    }
}
