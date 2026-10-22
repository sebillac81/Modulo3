using System.ComponentModel.DataAnnotations;
using RoastingManager.Api.Validation;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-03: registrar y consultar cafés verdes. AC-03b: el precio de compra por
// kilogramo debe ser mayor a cero.
public record CreateGreenCoffeeRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")] string Name,
    [Required(ErrorMessage = "El proveedor es obligatorio.")] int SupplierId,
    [GreaterThanZero(ErrorMessage = "El precio de compra debe ser mayor a cero.")] decimal PurchasePricePerKg
);

public record GreenCoffeeResponse(
    int Id,
    string Name,
    int SupplierId,
    string SupplierName,
    decimal PurchasePricePerKg
);
