namespace RoastingManager.Api.Contracts.Batches;

// RF-19 / AC-19: stock disponible de café tostado (pool único, RF-13).
public record RoastedCoffeeStockResponse(decimal QuantityKg, decimal WeightedAverageCostPerKg);
