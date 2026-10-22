using System.ComponentModel.DataAnnotations;

namespace RoastingManager.Api.Contracts.Catalogs;

// RF-06: registrar y consultar clientes.
public record CreateCustomerRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")] string Name
);

public record CustomerResponse(int Id, string Name);
