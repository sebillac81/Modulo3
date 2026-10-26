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

        // El tueste siempre pierde masa: el café tostado obtenido no puede
        // superar al café verde utilizado (merma negativa es físicamente
        // imposible y, si se permitiera, contaminaría el costo promedio
        // ponderado del pool para todas las ventas posteriores).
        if (request.RoastedCoffeeObtainedKg > request.GreenCoffeeUsedKg)
        {
            ModelState.AddModelError(
                nameof(request.RoastedCoffeeObtainedKg),
                "La cantidad de café tostado obtenido no puede ser mayor a la cantidad de café verde utilizada.");
            return ValidationProblem(ModelState);
        }

        // AC-11: porcentaje de merma.
        var shrinkagePercentage = Rounding.To2(
            (request.GreenCoffeeUsedKg - request.RoastedCoffeeObtainedKg) / request.GreenCoffeeUsedKg * 100m);

        // AC-12: costo del café tostado, con el precio de compra vigente fijado.
        var costPerKg = Rounding.To2(
            greenCoffee.PurchasePricePerKg * request.GreenCoffeeUsedKg / request.RoastedCoffeeObtainedKg);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        // AC-10/RF-08: descuento atómico. El UPDATE solo afecta una fila si
        // hay stock suficiente en ESE MOMENTO en la base (no el que leyó la
        // app antes); así dos batches concurrentes no pueden leer el mismo
        // stock, pasar ambos la validación y que el segundo SaveChanges
        // pise al primero (lost update / sobreventa bajo concurrencia).
        var rowsAffected = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE green_coffees
            SET stock_kg = stock_kg - {request.GreenCoffeeUsedKg}
            WHERE id = {greenCoffee.Id} AND stock_kg >= {request.GreenCoffeeUsedKg}
            """);

        if (rowsAffected == 0)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(
                nameof(request.GreenCoffeeUsedKg), "No existe stock suficiente de café verde.");
            return ValidationProblem(ModelState);
        }

        // RF-09/RF-13: incremento y costo promedio ponderado en una sola
        // expresión atómica, calculada sobre el valor ACTUAL de la fila
        // (no uno leído antes por la app), bajo el lock que toma el UPDATE.
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE roasted_coffee_stock
            SET weighted_average_cost_per_kg = ROUND(
                    ((quantity_kg * weighted_average_cost_per_kg) + ({request.RoastedCoffeeObtainedKg} * {costPerKg}))
                    / (quantity_kg + {request.RoastedCoffeeObtainedKg}), 2),
                quantity_kg = quantity_kg + {request.RoastedCoffeeObtainedKg}
            WHERE id = 1
            """);

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
        _dbContext.RoastBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

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
