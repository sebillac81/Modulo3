using System.ComponentModel.DataAnnotations;

namespace RoastingManager.Api.Validation;

// Valida que una fecha no sea futura (p.ej. fecha de tueste o de venta).
// No se expide sobre null: eso es responsabilidad de [Required] — DateOnly
// es un value type, así que sin [Required] + tipo nullable, una fecha
// vacía/faltante en el JSON cae silenciosamente en el default
// (DateOnly.MinValue = 0001-01-01) en vez de rechazarse.
public class NotInFutureAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value switch
        {
            null => true,
            DateOnly date => date <= DateOnly.FromDateTime(DateTime.UtcNow),
            _ => false,
        };
    }
}
