using System.ComponentModel.DataAnnotations;
using RoastingManager.Api.Validation;

namespace RoastingManager.Api.Contracts.Batches;

// RF-07 / AC-07a / AC-07b: registrar batch de tueste con un único café verde.
// AC-10: rechazar si no hay stock suficiente. Datos del proceso (humedad,
// densidad, desarrollo) opcionales.
public record CreateRoastBatchRequest(
    [Required(ErrorMessage = "El café verde es obligatorio.")] int GreenCoffeeId,
    [GreaterThanZero(ErrorMessage = "Las cantidades deben ser mayores a cero.")] decimal GreenCoffeeUsedKg,
    [GreaterThanZero(ErrorMessage = "Las cantidades deben ser mayores a cero.")] decimal RoastedCoffeeObtainedKg,
    [Required(ErrorMessage = "La fecha del tueste es obligatoria.")]
    [NotInFuture(ErrorMessage = "La fecha del tueste no puede ser futura.")]
    DateOnly? RoastDate,
    decimal? HumidityPercentage,
    decimal? Density,
    decimal? DevelopmentPercentage
);

// RF-11/RF-12/RF-20/RF-22: expone merma, costo fijado y precio fijado al
// momento del registro para el historial y la consulta de costo por batch.
public record RoastBatchResponse(
    int Id,
    int GreenCoffeeId,
    string GreenCoffeeName,
    decimal GreenCoffeeUsedKg,
    decimal RoastedCoffeeObtainedKg,
    DateOnly RoastDate,
    decimal? HumidityPercentage,
    decimal? Density,
    decimal? DevelopmentPercentage,
    decimal PurchasePricePerKgAtBatchTime,
    decimal ShrinkagePercentage,
    decimal CostPerKg
);
