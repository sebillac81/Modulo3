using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Data;
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
        var supplier = new CoffeeSupplier { Name = request.Name };
        _dbContext.CoffeeSuppliers.Add(supplier);
        await _dbContext.SaveChangesAsync();

        var response = new CoffeeSupplierResponse(supplier.Id, supplier.Name);
        return CreatedAtAction(nameof(GetAll), response);
    }
}
