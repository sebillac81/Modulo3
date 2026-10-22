using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace RoastingManager.Api.Tests;

// Base de datos dedicada a tests de integración, separada de la base de
// desarrollo/producción compartida entre módulos (ver docker-compose.yml).
// El esquema se migra una sola vez vía DbUp antes de correr los tests
// (ver README de tests); acá solo truncamos entre casos.
public class CatalogsApiFactory : WebApplicationFactory<Program>
{
    public const string TestConnectionString =
        "Host=localhost;Port=5432;Database=roasting_manager_test;Username=roasting_manager;Password=roasting_manager";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RoastingManager"] = TestConnectionString,
            });
        });
    }

    public static async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(TestConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            TRUNCATE sale_presentation_supplies, sale_presentations, supplies,
                     supply_suppliers, green_coffees, coffee_suppliers, customers
            RESTART IDENTITY CASCADE;
            """;
        await command.ExecuteNonQueryAsync();
    }
}
