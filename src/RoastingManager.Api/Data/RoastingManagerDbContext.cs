using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Data;

// DbContext para queries y mapeo de entidades vía EF Core.
// El esquema de la base de datos se versiona con DbUp (ver src/RoastingManager.Migrations),
// NO con `dotnet ef migrations`. Las entidades se van a ir agregando en cada funcionalidad.
public class RoastingManagerDbContext : DbContext
{
    public RoastingManagerDbContext(DbContextOptions<RoastingManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<CoffeeSupplier> CoffeeSuppliers => Set<CoffeeSupplier>();
    public DbSet<SupplySupplier> SupplySuppliers => Set<SupplySupplier>();
    public DbSet<GreenCoffee> GreenCoffees => Set<GreenCoffee>();
    public DbSet<Supply> Supplies => Set<Supply>();
    public DbSet<SalePresentation> SalePresentations => Set<SalePresentation>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalePresentation>()
            .HasMany(p => p.Supplies)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "sale_presentation_supplies",
                j => j.HasOne<Supply>().WithMany().HasForeignKey("supply_id"),
                j => j.HasOne<SalePresentation>().WithMany().HasForeignKey("presentation_id"),
                j =>
                {
                    j.ToTable("sale_presentation_supplies");
                    j.HasKey("presentation_id", "supply_id");
                });
    }
}
