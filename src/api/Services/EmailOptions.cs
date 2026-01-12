namespace Vorluno.Contacto.Api.Services;

public sealed class EmailOptions
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public Dictionary<string, string>? Routing { get; set; }

    public SmtpOptions Smtp { get; set; } = new();
    public AckOptions Ack { get; set; } = new();

    // Color de marca + logo para inlines
    public string? BrandColor { get; set; } = "#F47821";
    public LogoOptions? Logo { get; set; } = new();

    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
        public int TimeoutMs { get; set; } = 15000;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
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
        public string? File { get; set; }
        public string Cid { get; set; } = "cidImage";
    }

    public sealed class LogoOptions
    {
        // Ruta relativa al ContentRoot o absoluta
        public string? File { get; set; } = "wwwroot/email-assets/vorluno-logo.png";
        // Content-ID para usar en el HTML: <img src="cid:{Cid}">
        public string Cid { get; set; } = "vorlunoLogo";
    }
}
