using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Catalogs;

namespace RoastingManager.Api.Tests.Catalogs;

// RF-03 / AC-03a / AC-03b: registrar y consultar cafés verdes.
public class GreenCoffeesTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public GreenCoffeesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => ApiFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<int> CreateSupplierAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/coffee-suppliers", new CreateCoffeeSupplierRequest("Finca La Esperanza"));
        var supplier = await response.Content.ReadFromJsonAsync<CoffeeSupplierResponse>();
        return supplier!.Id;
    }

    [Fact]
    public async Task AC03a_RegistrarCafeVerde_ConPrecioValido_GuardaElRegistro()
    {
        var supplierId = await CreateSupplierAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/green-coffees",
            new CreateGreenCoffeeRequest("Colombia Huila", supplierId, 5.50m, 10m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GreenCoffeeResponse>();
        Assert.Equal(5.50m, body!.PurchasePricePerKg);
        Assert.Equal("Finca La Esperanza", body.SupplierName);
        Assert.Equal(10m, body.StockKg);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AC03b_RegistrarCafeVerde_ConPrecioInvalido_RechazaLaOperacion(decimal invalidPrice)
    {
        var supplierId = await CreateSupplierAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/green-coffees",
            new CreateGreenCoffeeRequest("Colombia Huila", supplierId, invalidPrice, 10m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("precio de compra debe ser mayor a cero", body);
    }

    // Reportado: un nombre de 5000 caracteres daba 500 con stack trace en
    // vez de 400 (supera el varchar(200) de la DB).
    [Fact]
    public async Task RechazaNombreDemasiadoLargo_Con400YSinStackTrace()
    {
        var supplierId = await CreateSupplierAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/green-coffees",
            new CreateGreenCoffeeRequest(new string('a', 5000), supplierId, 5.50m, 10m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("no puede superar los 200 caracteres", body);
        Assert.DoesNotContain("StackTrace", body);
        Assert.DoesNotContain("Npgsql", body);
    }
}
