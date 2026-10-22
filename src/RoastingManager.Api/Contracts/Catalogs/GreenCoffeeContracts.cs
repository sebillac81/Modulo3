using System.ComponentModel.DataAnnotations;
using RoastingManager.Api.Validation;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-03: registrar y consultar cafés verdes. AC-03b: el precio de compra por
// kilogramo debe ser mayor a cero. StockKg: cantidad disponible al registrar
// (RF-08/RF-10/RNF-02) — no hay gestión de compras en el alcance, así que es
// el único punto de entrada del stock de café verde.
public record CreateGreenCoffeeRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")] string Name,
    [Required(ErrorMessage = "El proveedor es obligatorio.")] int SupplierId,
    [GreaterThanZero(ErrorMessage = "El precio de compra debe ser mayor a cero.")] decimal PurchasePricePerKg,
    [GreaterThanZero(ErrorMessage = "El stock debe ser mayor a cero.")] decimal StockKg
);

public record GreenCoffeeResponse(
    int Id,
    string Name,
    int SupplierId,
    string SupplierName,
    decimal PurchasePricePerKg,
    decimal StockKg
);
