namespace RoastingManager.Api.Domain;

// RNF-03: cálculos monetarios y porcentuales con precisión de 2 decimales.
public static class Rounding
{
    public static decimal To2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
