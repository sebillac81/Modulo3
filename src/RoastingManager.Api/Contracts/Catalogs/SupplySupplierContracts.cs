using System.ComponentModel.DataAnnotations;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-02: registrar y consultar proveedores de insumos.
public record CreateSupplySupplierRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    string Name
);

public record SupplySupplierResponse(int Id, string Name);
