using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Sales;
using RoastingManager.Api.Data;
using RoastingManager.Api.Domain;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Controllers.Sales;

// RF-14 a RF-18 / RF-21 / RF-23 / RF-24: registrar y consultar ventas.
[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public SalesController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // RF-21 / AC-21: historial de ventas, más recientes primero.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleResponse>>> GetAll()
    {
        var sales = await _dbContext.Sales
            .Include(s => s.Customer)
            .Include(s => s.Presentation)
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.Id)
            .ToListAsync();

        return Ok(sales.Select(s => ToResponse(s)));
    }

    // RF-15/RF-16/RF-17/RF-18.
    [HttpPost]
    public async Task<ActionResult<SaleResponse>> Create(CreateSaleRequest request)
    {
        var customer = await _dbContext.Customers.FindAsync(request.CustomerId);
        if (customer is null)
        {
            ModelState.AddModelError(nameof(request.CustomerId), "El cliente indicado no existe.");
            return ValidationProblem(ModelState);
        }

        var presentation = await _dbContext.SalePresentations
            .Include(p => p.Supplies)
            .FirstOrDefaultAsync(p => p.Id == request.PresentationId);
        if (presentation is null)
        {
            ModelState.AddModelError(nameof(request.PresentationId), "La presentación indicada no existe.");
            return ValidationProblem(ModelState);
        }

        // La presentación describe el tamaño en gramos; el stock/costo de
        // café tostado se maneja en kg (ver Funcionalidad 2).
        var roastedCoffeeKgSold = request.UnitsSold * presentation.RoastedCoffeeGrams / 1000m;

        // AC-17: costos asociados, con los precios de insumos vigentes fijados.
        var associatedCosts = Rounding.To2(
            request.UnitsSold * presentation.Supplies.Sum(s => s.UnitPrice));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        // AC-16/RF-15: descuento atómico. El UPDATE solo afecta una fila si
        // hay stock suficiente en ESE MOMENTO en la base (no el que leyó la
        // app antes); así dos ventas concurrentes no pueden leer el mismo
        // stock, pasar ambas la validación y que la segunda SaveChanges pise
        // a la primera (lost update / sobreventa bajo concurrencia).
        var rowsAffected = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE roasted_coffee_stock
            SET quantity_kg = quantity_kg - {roastedCoffeeKgSold}
            WHERE id = 1 AND quantity_kg >= {roastedCoffeeKgSold}
            """);

        if (rowsAffected == 0)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(
                nameof(request.UnitsSold), "No existe stock suficiente de café tostado.");
            return ValidationProblem(ModelState);
        }

        // AC-18: margen de ganancia, con el costo promedio ponderado vigente
        // fijado. Vender no modifica el costo promedio del pool, solo la
        // cantidad; lo leemos ya dentro de la transacción (no antes del
        // descuento), para no depender de un valor leído por la app.
        var roastedCoffeeCostAtSaleTime = await _dbContext.RoastedCoffeeStock
            .AsNoTracking()
            .Where(s => s.Id == 1)
            .Select(s => s.WeightedAverageCostPerKg)
            .SingleAsync();

        var revenue = request.UnitSalePrice * request.UnitsSold;
        var cost = (roastedCoffeeCostAtSaleTime * roastedCoffeeKgSold) + associatedCosts;
        var profitMarginPercentage = Rounding.To2((revenue - cost) / revenue * 100m);

        var sale = new Sale
        {
            CustomerId = customer.Id,
            PresentationId = presentation.Id,
            UnitsSold = request.UnitsSold,
            UnitSalePrice = request.UnitSalePrice,
            SaleDate = request.SaleDate!.Value,
            RoastedCoffeeKgSold = roastedCoffeeKgSold,
            RoastedCoffeeCostPerKgAtSaleTime = roastedCoffeeCostAtSaleTime,
            AssociatedCosts = associatedCosts,
            ProfitMarginPercentage = profitMarginPercentage,
        };
        _dbContext.Sales.Add(sale);
        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return CreatedAtAction(nameof(GetAll), ToResponse(sale, customer.Name, presentation.Name));
    }

    private static SaleResponse ToResponse(Sale sale, string? customerName = null, string? presentationName = null) =>
        new(
            sale.Id,
            sale.CustomerId,
            customerName ?? sale.Customer!.Name,
            sale.PresentationId,
            presentationName ?? sale.Presentation!.Name,
            sale.UnitsSold,
            sale.UnitSalePrice,
            sale.SaleDate,
            sale.RoastedCoffeeKgSold,
            sale.RoastedCoffeeCostPerKgAtSaleTime,
            sale.AssociatedCosts,
            sale.ProfitMarginPercentage);
}
