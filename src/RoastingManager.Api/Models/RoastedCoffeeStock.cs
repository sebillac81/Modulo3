namespace RoastingManager.Api.Models;

// RF-09/RF-13: pool único de café tostado (sin FIFO, ver Fuera de Alcance),
// con costo promedio ponderado. Fila única (Id = 1).
public class RoastedCoffeeStock
{
    public int Id { get; set; }
    public decimal QuantityKg { get; set; }
    public decimal WeightedAverageCostPerKg { get; set; }
}
