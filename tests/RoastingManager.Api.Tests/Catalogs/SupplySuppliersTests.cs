using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Catalogs;

namespace RoastingManager.Api.Tests.Catalogs;

// RF-02 / AC-02: registrar y consultar proveedores de insumos.
public class SupplySuppliersTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public SupplySuppliersTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => ApiFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AC02_RegistrarProveedorDeInsumos_GuardaElRegistro()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/supply-suppliers", new CreateSupplySupplierRequest("Bolsas SRL"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var listResponse = await _client.GetFromJsonAsync<List<SupplySupplierResponse>>("/api/supply-suppliers");
        Assert.Contains(listResponse!, s => s.Name == "Bolsas SRL");
    }
}
