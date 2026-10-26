using System.ComponentModel.DataAnnotations;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-06: registrar y consultar clientes.
public record CreateCustomerRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    string Name
);

public record CustomerResponse(int Id, string Name);
