using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Catalogs;

namespace RoastingManager.Api.Tests.Catalogs;

// RF-04 / AC-04a / AC-04b: registrar y consultar insumos.
public class SuppliesTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public SuppliesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => ApiFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<int> CreateSupplierAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/supply-suppliers", new CreateSupplySupplierRequest("Bolsas SRL"));
        var supplier = await response.Content.ReadFromJsonAsync<SupplySupplierResponse>();
        return supplier!.Id;
    }

    [Fact]
    public async Task AC04a_RegistrarInsumo_ConPrecioValido_GuardaElRegistro()
    {
        var supplierId = await CreateSupplierAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/supplies",
            new CreateSupplyRequest("Bolsa 250g", supplierId, 12.30m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SupplyResponse>();
        Assert.Equal(12.30m, body!.UnitPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task AC04b_RegistrarInsumo_ConPrecioInvalido_RechazaLaOperacion(decimal invalidPrice)
    {
        var supplierId = await CreateSupplierAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/supplies",
            new CreateSupplyRequest("Bolsa 250g", supplierId, invalidPrice));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("precio por unidad debe ser mayor a cero", body);
    }
}
