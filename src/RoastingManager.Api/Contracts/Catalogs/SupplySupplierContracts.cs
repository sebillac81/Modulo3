using System.ComponentModel.DataAnnotations;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-02: registrar y consultar proveedores de insumos.
public record CreateSupplySupplierRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")] string Name
);

public record SupplySupplierResponse(int Id, string Name);
