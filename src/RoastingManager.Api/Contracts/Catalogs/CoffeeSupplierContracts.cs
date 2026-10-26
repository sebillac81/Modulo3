using System.ComponentModel.DataAnnotations;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-01: registrar y consultar proveedores de café.
public record CreateCoffeeSupplierRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    string Name
);

public record CoffeeSupplierResponse(int Id, string Name);
