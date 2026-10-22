using Microsoft.EntityFrameworkCore;

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
}
