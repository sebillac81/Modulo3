using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Data;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Controllers.Catalogs;

// RF-03 / AC-03a / AC-03b: registrar y consultar cafés verdes.
[ApiController]
[Route("api/green-coffees")]
public class GreenCoffeesController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public GreenCoffeesController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GreenCoffeeResponse>>> GetAll()
    {
        var greenCoffees = await _dbContext.GreenCoffees
            .Include(c => c.Supplier)
            .OrderBy(c => c.Name)
            .Select(c => new GreenCoffeeResponse(
                c.Id, c.Name, c.SupplierId, c.Supplier!.Name, c.PurchasePricePerKg, c.StockKg))
            .ToListAsync();

        return Ok(greenCoffees);
    }

    [HttpPost]
    public async Task<ActionResult<GreenCoffeeResponse>> Create(CreateGreenCoffeeRequest request)
    {
        var supplier = await _dbContext.CoffeeSuppliers.FindAsync(request.SupplierId);
        if (supplier is null)
        {
            ModelState.AddModelError(nameof(request.SupplierId), "El proveedor de café indicado no existe.");
            return ValidationProblem(ModelState);
        }

        var greenCoffee = new GreenCoffee
        {
            Name = request.Name,
            SupplierId = request.SupplierId,
            PurchasePricePerKg = request.PurchasePricePerKg,
            StockKg = request.StockKg,
        };
        _dbContext.GreenCoffees.Add(greenCoffee);
        await _dbContext.SaveChangesAsync();

        var response = new GreenCoffeeResponse(
            greenCoffee.Id, greenCoffee.Name, greenCoffee.SupplierId, supplier.Name,
            greenCoffee.PurchasePricePerKg, greenCoffee.StockKg);
        return CreatedAtAction(nameof(GetAll), response);
    }
}
