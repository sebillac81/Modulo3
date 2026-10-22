using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Data;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Controllers.Catalogs;

// RF-04 / AC-04a / AC-04b: registrar y consultar insumos.
[ApiController]
[Route("api/supplies")]
public class SuppliesController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public SuppliesController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplyResponse>>> GetAll()
    {
        var supplies = await _dbContext.Supplies
            .Include(s => s.Supplier)
            .OrderBy(s => s.Name)
            .Select(s => new SupplyResponse(s.Id, s.Name, s.SupplierId, s.Supplier!.Name, s.UnitPrice))
            .ToListAsync();

        return Ok(supplies);
    }

    [HttpPost]
    public async Task<ActionResult<SupplyResponse>> Create(CreateSupplyRequest request)
    {
        var supplier = await _dbContext.SupplySuppliers.FindAsync(request.SupplierId);
        if (supplier is null)
        {
            ModelState.AddModelError(nameof(request.SupplierId), "El proveedor de insumos indicado no existe.");
            return ValidationProblem(ModelState);
        }

        var supply = new Supply
        {
            Name = request.Name,
            SupplierId = request.SupplierId,
            UnitPrice = request.UnitPrice,
        };
        _dbContext.Supplies.Add(supply);
        await _dbContext.SaveChangesAsync();

        var response = new SupplyResponse(supply.Id, supply.Name, supply.SupplierId, supplier.Name, supply.UnitPrice);
        return CreatedAtAction(nameof(GetAll), response);
    }
}
