namespace RoastingManager.Api.Models;

// RF-03: café verde, con precio de compra por kilogramo (debe ser > 0, AC-03b).
public class GreenCoffee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public CoffeeSupplier? Supplier { get; set; }
    public decimal PurchasePricePerKg { get; set; }
}
