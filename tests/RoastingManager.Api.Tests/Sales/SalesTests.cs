using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Batches;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Contracts.Sales;

namespace RoastingManager.Api.Tests.Sales;

// RF-14 a RF-18, RF-21, RF-23, RF-24: registrar y consultar ventas.
public class SalesTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public SalesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => ApiFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // Deja 8kg de café tostado en stock a costo promedio ponderado 12.50/kg
    // (batch de 10kg verde a $10/kg -> 8kg tostado), y una presentación de
    // 250g con un insumo de $2.00/u.
    private async Task<(int CustomerId, int PresentationId)> SetUpScenarioAsync()
    {
        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers", new CreateCustomerRequest("Kiosco Centro"));
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var coffeeSupplierResponse = await _client.PostAsJsonAsync(
            "/api/coffee-suppliers", new CreateCoffeeSupplierRequest("Finca La Esperanza"));
        var coffeeSupplier = await coffeeSupplierResponse.Content.ReadFromJsonAsync<CoffeeSupplierResponse>();

        var greenCoffeeResponse = await _client.PostAsJsonAsync(
            "/api/green-coffees",
            new CreateGreenCoffeeRequest("Colombia Huila", coffeeSupplier!.Id, 10m, 100m));
        var greenCoffee = await greenCoffeeResponse.Content.ReadFromJsonAsync<GreenCoffeeResponse>();

        await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffee!.Id, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));

        var supplySupplierResponse = await _client.PostAsJsonAsync(
            "/api/supply-suppliers", new CreateSupplySupplierRequest("Bolsas SRL"));
        var supplySupplier = await supplySupplierResponse.Content.ReadFromJsonAsync<SupplySupplierResponse>();

        var supplyResponse = await _client.PostAsJsonAsync(
            "/api/supplies", new CreateSupplyRequest("Bolsa 250g", supplySupplier!.Id, 2.00m));
        var supply = await supplyResponse.Content.ReadFromJsonAsync<SupplyResponse>();

        var presentationResponse = await _client.PostAsJsonAsync(
            "/api/sale-presentations",
            new CreateSalePresentationRequest("Paquete 250g", 250m, new List<int> { supply!.Id }));
        var presentation = await presentationResponse.Content.ReadFromJsonAsync<SalePresentationResponse>();

        return (customer!.Id, presentation!.Id);
    }

    [Fact]
    public async Task AC14a_RegistrarVenta_ConDatosValidos_GuardaElRegistro()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2026, 10, 22)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SaleResponse>();
        Assert.Equal(4, body!.UnitsSold);
        Assert.Equal(20m, body.UnitSalePrice);
        Assert.Equal(1.00m, body.RoastedCoffeeKgSold); // 4 * 250g / 1000
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(4, 0)]
    [InlineData(-1, 20)]
    public async Task AC14b_RegistrarVenta_ConCantidadOPrecioInvalidos_RechazaLaOperacion(int units, decimal price)
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, units, price, new DateOnly(2026, 10, 22)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("cantidad y el precio deben ser mayores a cero", body);
    }

    [Fact]
    public async Task AC15_RegistrarVenta_DescuentaStockDeCafeTostado()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2026, 10, 22)));

        var stock = await _client.GetFromJsonAsync<RoastedCoffeeStockResponse>("/api/roasted-coffee-stock");
        Assert.Equal(7.00m, stock!.QuantityKg); // 8kg - 1kg vendido
        Assert.Equal(12.50m, stock.WeightedAverageCostPerKg); // no cambia al vender
    }

    [Fact]
    public async Task AC16_RegistrarVenta_SinStockSuficienteDeCafeTostado_RechazaLaOperacion()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        // Hay 8kg en stock; pedir 1000 unidades de 250g (250kg) excede el stock.
        var response = await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 1000, 20m, new DateOnly(2026, 10, 22)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("no existe stock suficiente", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AC17_RegistrarVenta_CalculaLosCostosAsociados()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2026, 10, 22)));

        var body = await response.Content.ReadFromJsonAsync<SaleResponse>();
        Assert.Equal(8.00m, body!.AssociatedCosts); // 4 unidades * $2.00 del insumo
    }

    [Fact]
    public async Task AC18_RegistrarVenta_CalculaElMargenDeGanancia()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2026, 10, 22)));

        var body = await response.Content.ReadFromJsonAsync<SaleResponse>();

        // revenue = 20*4 = 80; costo = 12.50*1.00 + 8.00 = 20.50
        // margen = (80-20.50)/80*100 = 74.375 -> 74.38
        Assert.Equal(74.38m, body!.ProfitMarginPercentage);
    }

    [Fact]
    public async Task AC21_ConsultarHistorialDeVentas_MuestraLasVentasRegistradas()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2026, 10, 22)));

        var history = await _client.GetFromJsonAsync<List<SaleResponse>>("/api/sales");

        Assert.Single(history!);
        Assert.Equal("Kiosco Centro", history![0].CustomerName);
        Assert.Equal("Paquete 250g", history[0].PresentationName);
    }

    [Fact]
    public async Task AC23_ConsultarCostosAsociadosDeUnaVenta_MuestraLosCostosCalculados()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2026, 10, 22)));
        var created = await response.Content.ReadFromJsonAsync<SaleResponse>();

        var history = await _client.GetFromJsonAsync<List<SaleResponse>>("/api/sales");
        var persisted = history!.Single(s => s.Id == created!.Id);

        Assert.Equal(8.00m, persisted.AssociatedCosts);
    }

    [Fact]
    public async Task AC24_ConsultarRentabilidadDeUnaVenta_MuestraElMargenCalculado()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2026, 10, 22)));
        var created = await response.Content.ReadFromJsonAsync<SaleResponse>();

        var history = await _client.GetFromJsonAsync<List<SaleResponse>>("/api/sales");
        var persisted = history!.Single(s => s.Id == created!.Id);

        Assert.Equal(74.38m, persisted.ProfitMarginPercentage);
    }

    // --- Regresión: condición de carrera en el descuento de stock de café tostado ---
    // Reportado: con 19.96kg en stock se intentaron vender 25kg (en varias
    // ventas concurrentes) y el stock bajó solo 5kg en vez de rechazar lo
    // que excedía el stock disponible. Mismo root cause que en batches:
    // lectura + validación en memoria + "SaveChanges" pisando el valor.

    [Fact]
    public async Task VentasConcurrentes_NoPermitenSobreventaDeStockDeCafeTostado()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync(); // deja 8kg en stock a $12.50/kg

        // Cada venta de 4 unidades de 250g consume 1kg; con 8kg en stock
        // entran como máximo 8 de las 10 solicitadas concurrentemente.
        var requests = Enumerable.Range(0, 10).Select(_ => _client.PostAsJsonAsync(
            "/api/sales",
            new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2026, 10, 22))));

        var responses = await Task.WhenAll(requests);

        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var rejectedCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        Assert.Equal(8, successCount);
        Assert.Equal(2, rejectedCount);

        var stock = await _client.GetFromJsonAsync<RoastedCoffeeStockResponse>("/api/roasted-coffee-stock");
        Assert.Equal(0.00m, stock!.QuantityKg); // nunca negativo ni "pisado" en un valor mayor
    }

    // --- Regresión: fechas vacías/futuras (ver también RoastBatchesTests) ---

    [Fact]
    public async Task RechazaVenta_ConFechaFaltante()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales", new CreateSaleRequest(customerId, presentationId, 4, 20m, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("fecha de la venta es obligatoria", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RechazaVenta_ConFechaFutura()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales", new CreateSaleRequest(customerId, presentationId, 4, 20m, new DateOnly(2099, 1, 1)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("no puede ser futura", body, StringComparison.OrdinalIgnoreCase);
    }

    // --- Regresión: margen muy negativo desbordaba numeric(6,2) y daba un
    // 500 con stack trace en vez de guardar la venta. ---
    [Fact]
    public async Task RegistrarVenta_ConPrecioMuyBajo_NoRompeConUnErrorDeServidor()
    {
        var (customerId, presentationId) = await SetUpScenarioAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/sales", new CreateSaleRequest(customerId, presentationId, 1, 0.01m, new DateOnly(2026, 10, 22)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SaleResponse>();
        Assert.True(body!.ProfitMarginPercentage < -1000m); // margen muy negativo, pero calculado y guardado
    }
}
