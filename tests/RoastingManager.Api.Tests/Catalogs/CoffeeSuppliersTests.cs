using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Catalogs;

namespace RoastingManager.Api.Tests.Catalogs;

// RF-01 / AC-01: registrar y consultar proveedores de café.
public class CoffeeSuppliersTests : IClassFixture<CatalogsApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public CoffeeSuppliersTests(CatalogsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => CatalogsApiFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AC01_RegistrarProveedorDeCafe_GuardaElRegistro()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/coffee-suppliers", new CreateCoffeeSupplierRequest("Finca La Esperanza"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var listResponse = await _client.GetFromJsonAsync<List<CoffeeSupplierResponse>>("/api/coffee-suppliers");
        Assert.Contains(listResponse!, s => s.Name == "Finca La Esperanza");
    }
}
