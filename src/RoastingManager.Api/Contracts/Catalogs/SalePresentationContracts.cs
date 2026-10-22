using System.ComponentModel.DataAnnotations;
using RoastingManager.Api.Validation;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-05: registrar y consultar presentaciones de venta. AC-05b: la cantidad de
// café tostado en gramos debe ser mayor a cero. Los insumos asociados son
// opcionales (cero o más).
public record CreateSalePresentationRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")] string Name,
    [GreaterThanZero(ErrorMessage = "La cantidad debe ser mayor a cero.")] decimal RoastedCoffeeGrams,
    List<int>? SupplyIds
);

public record SalePresentationResponse(
    int Id,
    string Name,
    decimal RoastedCoffeeGrams,
    List<SupplyResponse> Supplies
);
