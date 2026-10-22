using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Data;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Controllers.Catalogs;

// RF-05 / AC-05a / AC-05b: registrar y consultar presentaciones de venta.
[ApiController]
[Route("api/sale-presentations")]
public class SalePresentationsController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public SalePresentationsController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalePresentationResponse>>> GetAll()
    {
        var presentations = await _dbContext.SalePresentations
            .Include(p => p.Supplies)
            .ThenInclude(s => s.Supplier)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(presentations.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<SalePresentationResponse>> Create(CreateSalePresentationRequest request)
    {
        var supplyIds = request.SupplyIds ?? new List<int>();
        var supplies = await _dbContext.Supplies
            .Include(s => s.Supplier)
            .Where(s => supplyIds.Contains(s.Id))
            .ToListAsync();

        var missingIds = supplyIds.Except(supplies.Select(s => s.Id)).ToList();
        if (missingIds.Count > 0)
        {
            ModelState.AddModelError(
                nameof(request.SupplyIds),
                $"Los siguientes insumos no existen: {string.Join(", ", missingIds)}.");
            return ValidationProblem(ModelState);
        }

        var presentation = new SalePresentation
        {
            Name = request.Name,
            RoastedCoffeeGrams = request.RoastedCoffeeGrams,
            Supplies = supplies,
        };
        _dbContext.SalePresentations.Add(presentation);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), ToResponse(presentation));
    }

    private static SalePresentationResponse ToResponse(SalePresentation presentation) =>
        new(
            presentation.Id,
            presentation.Name,
            presentation.RoastedCoffeeGrams,
            presentation.Supplies
                .Select(s => new SupplyResponse(s.Id, s.Name, s.SupplierId, s.Supplier!.Name, s.UnitPrice))
                .ToList());
}
