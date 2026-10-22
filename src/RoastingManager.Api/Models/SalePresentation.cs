namespace RoastingManager.Api.Models;

// RF-05: presentación de venta (cantidad de café tostado en gramos, debe ser > 0,
// AC-05b) con cero o más insumos asociados (p.ej. bolsa y etiqueta).
public class SalePresentation
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal RoastedCoffeeGrams { get; set; }
    public List<Supply> Supplies { get; set; } = new();
}
