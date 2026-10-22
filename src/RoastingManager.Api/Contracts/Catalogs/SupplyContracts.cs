using System.ComponentModel.DataAnnotations;
using RoastingManager.Api.Validation;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-04: registrar y consultar insumos. AC-04b: el precio por unidad debe ser
// mayor a cero.
public record CreateSupplyRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")] string Name,
    [Required(ErrorMessage = "El proveedor es obligatorio.")] int SupplierId,
    [GreaterThanZero(ErrorMessage = "El precio por unidad debe ser mayor a cero.")] decimal UnitPrice
);

public record SupplyResponse(
    int Id,
    string Name,
    int SupplierId,
    string SupplierName,
    decimal UnitPrice
);
