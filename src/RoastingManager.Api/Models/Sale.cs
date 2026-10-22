namespace RoastingManager.Api.Models;

// RF-14: venta de café tostado a un cliente, según una presentación.
// RF-17/RF-18: costos asociados y margen se calculan y fijan al registrar
// la venta (mismo criterio que AC-12 para batches): no se recalculan ante
// cambios posteriores de precios en el catálogo ni del costo del pool.
public class Sale
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int PresentationId { get; set; }
    public SalePresentation? Presentation { get; set; }
    public int UnitsSold { get; set; }
    public decimal UnitSalePrice { get; set; }
    public DateOnly SaleDate { get; set; }

    // Derivados y fijados al momento del registro.
    public decimal RoastedCoffeeKgSold { get; set; }
    public decimal RoastedCoffeeCostPerKgAtSaleTime { get; set; }
    public decimal AssociatedCosts { get; set; }
    public decimal ProfitMarginPercentage { get; set; }
}
