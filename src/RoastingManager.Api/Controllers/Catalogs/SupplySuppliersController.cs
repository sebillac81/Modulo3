using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Data;
using RoastingManager.Api.Domain;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Controllers.Catalogs;

// RF-02 / AC-02: registrar y consultar proveedores de insumos.
[ApiController]
[Route("api/supply-suppliers")]
public class SupplySuppliersController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public SupplySuppliersController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplySupplierResponse>>> GetAll()
    {
        var suppliers = await _dbContext.SupplySuppliers
            .OrderBy(s => s.Name)
            .Select(s => new SupplySupplierResponse(s.Id, s.Name))
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpPost]
    public async Task<ActionResult<SupplySupplierResponse>> Create(CreateSupplySupplierRequest request)
    {
        // Un proveedor "duplicado" con otra capitalización (ej. "Bolsas
        // SRL" vs "BOLSAS SRL") es casi siempre un error de tipeo sobre la
        // misma entidad real, no dos proveedores distintos.
        var alreadyExists = await _dbContext.SupplySuppliers
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower());
        if (alreadyExists)
        {
            ModelState.AddModelError(nameof(request.Name), "Ya existe un proveedor de insumos con ese nombre.");
            return ValidationProblem(ModelState);
        }

        var supplier = new SupplySupplier { Name = request.Name };
        _dbContext.SupplySuppliers.Add(supplier);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Dos requests concurrentes pudieron pasar el chequeo anterior
            // antes de que ninguno escribiera (misma lógica que el fix de
            // stock); el índice único de la DB es la guarda definitiva.
            ModelState.AddModelError(nameof(request.Name), "Ya existe un proveedor de insumos con ese nombre.");
            return ValidationProblem(ModelState);
        }

        var response = new SupplySupplierResponse(supplier.Id, supplier.Name);
        return CreatedAtAction(nameof(GetAll), response);
    }
}
