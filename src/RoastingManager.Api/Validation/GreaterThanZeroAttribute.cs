using System.ComponentModel.DataAnnotations;

namespace RoastingManager.Api.Validation;

// Valida que un valor numérico sea estrictamente mayor a cero.
// Usado por RF-03/RF-04/RF-05 (precios y cantidades deben ser > 0, AC-03b/AC-04b/AC-05b).
public class GreaterThanZeroAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value switch
        {
            decimal d => d > 0,
            int i => i > 0,
            double db => db > 0,
            _ => false,
        };
    }
}
