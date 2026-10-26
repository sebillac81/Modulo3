using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Data;
using RoastingManager.Api.Domain;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Controllers.Catalogs;

// RF-01 / AC-01: registrar y consultar proveedores de café.
[ApiController]
[Route("api/coffee-suppliers")]
public class CoffeeSuppliersController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public CoffeeSuppliersController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoffeeSupplierResponse>>> GetAll()
    {
        var suppliers = await _dbContext.CoffeeSuppliers
            .OrderBy(s => s.Name)
            .Select(s => new CoffeeSupplierResponse(s.Id, s.Name))
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpPost]
    public async Task<ActionResult<CoffeeSupplierResponse>> Create(CreateCoffeeSupplierRequest request)
    {
        // Un proveedor "duplicado" con otra capitalización (ej. "Finca
        // Kenia" vs "FINCA KENIA") es casi siempre un error de tipeo sobre
        // la misma entidad real, no dos proveedores distintos.
        var alreadyExists = await _dbContext.CoffeeSuppliers
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower());
        if (alreadyExists)
        {
            ModelState.AddModelError(nameof(request.Name), "Ya existe un proveedor de café con ese nombre.");
            return ValidationProblem(ModelState);
        }

        var supplier = new CoffeeSupplier { Name = request.Name };
        _dbContext.CoffeeSuppliers.Add(supplier);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Dos requests concurrentes pudieron pasar el chequeo anterior
            // antes de que ninguno escribiera (misma lógica que el fix de
            // stock); el índice único de la DB es la guarda definitiva.
            ModelState.AddModelError(nameof(request.Name), "Ya existe un proveedor de café con ese nombre.");
            return ValidationProblem(ModelState);
        }

        var response = new CoffeeSupplierResponse(supplier.Id, supplier.Name);
        return CreatedAtAction(nameof(GetAll), response);
    }
}
