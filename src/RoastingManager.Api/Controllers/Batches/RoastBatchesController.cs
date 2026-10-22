using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Batches;
using RoastingManager.Api.Data;
using RoastingManager.Api.Domain;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Controllers.Batches;

// RF-07 a RF-13 / RF-20 / RF-22: registrar y consultar batches de tueste.
[ApiController]
[Route("api/roast-batches")]
public class RoastBatchesController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public RoastBatchesController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // RF-20 / AC-20: historial de batches, más recientes primero.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoastBatchResponse>>> GetAll()
    {
        var batches = await _dbContext.RoastBatches
            .Include(b => b.GreenCoffee)
            .OrderByDescending(b => b.RoastDate)
            .ThenByDescending(b => b.Id)
            .ToListAsync();

        return Ok(batches.Select(b => ToResponse(b)));
    }

    // RF-07/RF-08/RF-09/RF-10/RF-11/RF-12/RF-13.
    [HttpPost]
    public async Task<ActionResult<RoastBatchResponse>> Create(CreateRoastBatchRequest request)
    {
        var greenCoffee = await _dbContext.GreenCoffees.FindAsync(request.GreenCoffeeId);
        if (greenCoffee is null)
        {
            ModelState.AddModelError(nameof(request.GreenCoffeeId), "El café verde indicado no existe.");
            return ValidationProblem(ModelState);
        }

        // AC-10: no permitir el batch si no hay stock suficiente de café verde.
        if (greenCoffee.StockKg < request.GreenCoffeeUsedKg)
        {
            ModelState.AddModelError(
                nameof(request.GreenCoffeeUsedKg), "No existe stock suficiente de café verde.");
            return ValidationProblem(ModelState);
        }

        // AC-11: porcentaje de merma.
        var shrinkagePercentage = Rounding.To2(
            (request.GreenCoffeeUsedKg - request.RoastedCoffeeObtainedKg) / request.GreenCoffeeUsedKg * 100m);

        // AC-12: costo del café tostado, con el precio de compra vigente fijado.
        var costPerKg = Rounding.To2(
            greenCoffee.PurchasePricePerKg * request.GreenCoffeeUsedKg / request.RoastedCoffeeObtainedKg);

        var batch = new RoastBatch
        {
            GreenCoffeeId = greenCoffee.Id,
            GreenCoffeeUsedKg = request.GreenCoffeeUsedKg,
            RoastedCoffeeObtainedKg = request.RoastedCoffeeObtainedKg,
            RoastDate = request.RoastDate,
            HumidityPercentage = request.HumidityPercentage,
            Density = request.Density,
            DevelopmentPercentage = request.DevelopmentPercentage,
            PurchasePricePerKgAtBatchTime = greenCoffee.PurchasePricePerKg,
            ShrinkagePercentage = shrinkagePercentage,
            CostPerKg = costPerKg,
        };

        // RF-08: descontar stock de café verde.
        greenCoffee.StockKg -= request.GreenCoffeeUsedKg;

        // RF-09/RF-13: incrementar stock de café tostado y actualizar el
        // costo promedio ponderado del pool.
        var stock = await _dbContext.RoastedCoffeeStock.SingleAsync();
        var newQuantityKg = stock.QuantityKg + request.RoastedCoffeeObtainedKg;
        stock.WeightedAverageCostPerKg = Rounding.To2(
            ((stock.QuantityKg * stock.WeightedAverageCostPerKg) + (request.RoastedCoffeeObtainedKg * costPerKg))
            / newQuantityKg);
        stock.QuantityKg = newQuantityKg;

        _dbContext.RoastBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), ToResponse(batch, greenCoffee.Name));
    }

    private static RoastBatchResponse ToResponse(RoastBatch batch, string? greenCoffeeName = null) =>
        new(
            batch.Id,
            batch.GreenCoffeeId,
            greenCoffeeName ?? batch.GreenCoffee!.Name,
            batch.GreenCoffeeUsedKg,
            batch.RoastedCoffeeObtainedKg,
            batch.RoastDate,
            batch.HumidityPercentage,
            batch.Density,
            batch.DevelopmentPercentage,
            batch.PurchasePricePerKgAtBatchTime,
            batch.ShrinkagePercentage,
            batch.CostPerKg);
}
