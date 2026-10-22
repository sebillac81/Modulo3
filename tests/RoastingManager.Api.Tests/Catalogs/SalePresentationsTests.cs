using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Catalogs;

namespace RoastingManager.Api.Tests.Catalogs;

// RF-05 / AC-05a / AC-05b: registrar y consultar presentaciones de venta.
public class SalePresentationsTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public SalePresentationsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => ApiFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<int> CreateSupplyAsync()
    {
        var supplierResponse = await _client.PostAsJsonAsync(
            "/api/supply-suppliers", new CreateSupplySupplierRequest("Bolsas SRL"));
        var supplier = await supplierResponse.Content.ReadFromJsonAsync<SupplySupplierResponse>();

        var supplyResponse = await _client.PostAsJsonAsync(
            "/api/supplies", new CreateSupplyRequest("Bolsa 250g", supplier!.Id, 12.30m));
        var supply = await supplyResponse.Content.ReadFromJsonAsync<SupplyResponse>();
        return supply!.Id;
    }

    [Fact]
    public async Task AC05a_RegistrarPresentacion_ConCantidadValidaYInsumos_GuardaElRegistro()
    {
        var supplyId = await CreateSupplyAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sale-presentations",
            new CreateSalePresentationRequest("Paquete 250g", 250m, new List<int> { supplyId }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SalePresentationResponse>();
        Assert.Equal(250m, body!.RoastedCoffeeGrams);
        Assert.Single(body.Supplies);
    }

    [Fact]
    public async Task AC05a_RegistrarPresentacion_SinInsumos_GuardaElRegistro()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/sale-presentations",
            new CreateSalePresentationRequest("Granel 1kg", 1000m, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SalePresentationResponse>();
        Assert.Empty(body!.Supplies);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task AC05b_RegistrarPresentacion_ConCantidadInvalida_RechazaLaOperacion(decimal invalidGrams)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/sale-presentations",
            new CreateSalePresentationRequest("Paquete inválido", invalidGrams, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("cantidad debe ser mayor a cero", body);
    }
}
