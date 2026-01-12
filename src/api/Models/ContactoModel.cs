using System.ComponentModel.DataAnnotations;

namespace Vorluno.Contacto.Api.Models;

/// <summary>
/// Modelo simplificado para formulario de contacto/leads de Vorluno.
/// </summary>
public class ContactoModel : IValidatableObject
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido.")]
    [Phone(ErrorMessage = "Teléfono inválido.")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "El teléfono debe tener entre 8 y 20 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Empresa { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un tipo de proyecto.")]
    public string TipoProyecto { get; set; } = string.Empty;

    public string? Presupuesto { get; set; }

    public string? Urgencia { get; set; }

    [Required(ErrorMessage = "Por favor describe tu proyecto.")]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 1000 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    public string? Fuente { get; set; }

    [RequiredTrue(ErrorMessage = "Debe aceptar el aviso de privacidad.")]
    public bool AceptaPrivacidad { get; set; }

    /// <summary>Honeypot anti-bot (debe permanecer vacío).</summary>
    public string? CodigoInterno { get; set; }

    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Anti-bot check
        if (!string.IsNullOrWhiteSpace(CodigoInterno))
        {
            yield return new ValidationResult(
                "Envío rechazado por verificación anti-bot.",
                new[] { nameof(CodigoInterno) });
        }

        // Validar tipo de proyecto
        var tiposValidos = new[] { "web", "mobile", "ecommerce", "api", "consultoria", "otro" };
        if (!string.IsNullOrEmpty(TipoProyecto) &&
            !tiposValidos.Contains(TipoProyecto.ToLowerInvariant()))
        {
            yield return new ValidationResult(
                "Tipo de proyecto inválido.",
                new[] { nameof(TipoProyecto) });
        }

        // Validar presupuesto (opcional)
        if (!string.IsNullOrEmpty(Presupuesto))
        {
            var presupuestosValidos = new[] { "menos-5k", "5k-15k", "15k-50k", "mas-50k" };
            if (!presupuestosValidos.Contains(Presupuesto.ToLowerInvariant()))
            {
                yield return new ValidationResult(
                    "Presupuesto inválido.",
                    new[] { nameof(Presupuesto) });
            }
        }

        // Validar urgencia (opcional)
        if (!string.IsNullOrEmpty(Urgencia))
        {
            var urgenciasValidas = new[] { "inmediata", "1-2-semanas", "1-3-meses", "explorando" };
            if (!urgenciasValidas.Contains(Urgencia.ToLowerInvariant()))
            {
                yield return new ValidationResult(
                    "Urgencia inválida.",
                    new[] { nameof(Urgencia) });
            }
        }

        // Validar fuente (opcional)
        if (!string.IsNullOrEmpty(Fuente))
        {
            var fuentesValidas = new[] { "google", "linkedin", "referido", "otro" };
            if (!fuentesValidas.Contains(Fuente.ToLowerInvariant()))
            {
                yield return new ValidationResult(
                    "Fuente inválida.",
                    new[] { nameof(Fuente) });
            }
        }
    }
}

/// <summary>"Checkbox obligatorio".</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class RequiredTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is bool b && b;
}
