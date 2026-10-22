namespace RoastingManager.Api.Models;

// RF-07: batch de tueste, utilizando un único café verde.
// RF-11/RF-12: merma y costo se calculan y fijan al registrar el batch.
public class RoastBatch
{
    public int Id { get; set; }
    public int GreenCoffeeId { get; set; }
    public GreenCoffee? GreenCoffee { get; set; }
    public decimal GreenCoffeeUsedKg { get; set; }
    public decimal RoastedCoffeeObtainedKg { get; set; }
    public DateOnly RoastDate { get; set; }

    // Datos del proceso, opcionales (RF-07).
    public decimal? HumidityPercentage { get; set; }
    public decimal? Density { get; set; }
    public decimal? DevelopmentPercentage { get; set; }

    // Fijados al momento del registro (AC-12): no se recalculan ante cambios
    // posteriores del precio en el catálogo.
    public decimal PurchasePricePerKgAtBatchTime { get; set; }
    public decimal ShrinkagePercentage { get; set; }
    public decimal CostPerKg { get; set; }
}
