using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Batches;
using RoastingManager.Api.Data;

namespace RoastingManager.Api.Controllers.Batches;

// RF-19 / AC-19: consultar el stock disponible de café tostado (pool único).
[ApiController]
[Route("api/roasted-coffee-stock")]
public class RoastedCoffeeStockController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public RoastedCoffeeStockController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<RoastedCoffeeStockResponse>> Get()
    {
        var stock = await _dbContext.RoastedCoffeeStock.SingleAsync();
        return Ok(new RoastedCoffeeStockResponse(stock.QuantityKg, stock.WeightedAverageCostPerKg));
    }
}
