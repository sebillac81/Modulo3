namespace RoastingManager.Api.Models;

// RF-04: insumo (bolsa, etiqueta, etc.), con precio por unidad (debe ser > 0, AC-04b).
public class Supply
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public SupplySupplier? Supplier { get; set; }
    public decimal UnitPrice { get; set; }
}
