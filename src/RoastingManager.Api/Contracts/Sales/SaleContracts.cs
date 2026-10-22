using System.ComponentModel.DataAnnotations;
using RoastingManager.Api.Validation;

namespace RoastingManager.Api.Contracts.Sales;

// RF-14 / AC-14a / AC-14b: registrar venta de café tostado.
// AC-16: rechazar si no hay stock de café tostado suficiente.
public record CreateSaleRequest(
    [Required(ErrorMessage = "El cliente es obligatorio.")] int CustomerId,
    [Required(ErrorMessage = "La presentación es obligatoria.")] int PresentationId,
    [GreaterThanZero(ErrorMessage = "La cantidad y el precio deben ser mayores a cero.")] int UnitsSold,
    [GreaterThanZero(ErrorMessage = "La cantidad y el precio deben ser mayores a cero.")] decimal UnitSalePrice,
    DateOnly SaleDate
);

// RF-21/RF-23/RF-24: expone costos asociados y margen, fijados al momento
// del registro, para el historial y la consulta de rentabilidad por venta.
public record SaleResponse(
    int Id,
    int CustomerId,
    string CustomerName,
    int PresentationId,
    string PresentationName,
    int UnitsSold,
    decimal UnitSalePrice,
    DateOnly SaleDate,
    decimal RoastedCoffeeKgSold,
    decimal RoastedCoffeeCostPerKgAtSaleTime,
    decimal AssociatedCosts,
    decimal ProfitMarginPercentage
);
