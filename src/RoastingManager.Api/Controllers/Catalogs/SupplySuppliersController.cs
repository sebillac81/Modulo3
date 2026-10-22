using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Data;
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
        var supplier = new SupplySupplier { Name = request.Name };
        _dbContext.SupplySuppliers.Add(supplier);
        await _dbContext.SaveChangesAsync();

        var response = new SupplySupplierResponse(supplier.Id, supplier.Name);
        return CreatedAtAction(nameof(GetAll), response);
    }
}
