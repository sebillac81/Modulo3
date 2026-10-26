using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Catalogs;

namespace RoastingManager.Api.Tests.Catalogs;

// RF-01 / AC-01: registrar y consultar proveedores de café.
public class CoffeeSuppliersTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public CoffeeSuppliersTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => ApiFactory.ResetDatabaseAsync();

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

    // Reportado: "Finca Kenia" y "FINCA KENIA" entraban como dos
    // proveedores distintos.
    [Fact]
    public async Task RechazaProveedorDuplicado_ConDistintaCapitalizacion()
    {
        await _client.PostAsJsonAsync("/api/coffee-suppliers", new CreateCoffeeSupplierRequest("Finca Kenia"));

        var response = await _client.PostAsJsonAsync(
            "/api/coffee-suppliers", new CreateCoffeeSupplierRequest("FINCA KENIA"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("ya existe un proveedor de café con ese nombre", body, StringComparison.OrdinalIgnoreCase);

        var listResponse = await _client.GetFromJsonAsync<List<CoffeeSupplierResponse>>("/api/coffee-suppliers");
        Assert.Single(listResponse!);
    }

    // Reportado: un nombre de 5000 caracteres (supera el varchar(200) de la
    // DB) daba 500 con stack trace en vez de 400.
    [Fact]
    public async Task RechazaNombreDemasiadoLargo_Con400YSinStackTrace()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/coffee-suppliers", new CreateCoffeeSupplierRequest(new string('a', 5000)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("no puede superar los 200 caracteres", body);
        Assert.DoesNotContain("StackTrace", body);
        Assert.DoesNotContain("Npgsql", body);
    }
}
