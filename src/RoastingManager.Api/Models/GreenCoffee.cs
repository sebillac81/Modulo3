namespace RoastingManager.Api.Models;

// RF-03: café verde, con precio de compra por kilogramo (debe ser > 0, AC-03b).
// RF-08/RF-10/RNF-02: StockKg se fija al registrar (no hay gestión de compras
// en el alcance) y solo se decrementa al registrar batches de tueste.
public class GreenCoffee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public CoffeeSupplier? Supplier { get; set; }
    public decimal PurchasePricePerKg { get; set; }
    public decimal StockKg { get; set; }
}
