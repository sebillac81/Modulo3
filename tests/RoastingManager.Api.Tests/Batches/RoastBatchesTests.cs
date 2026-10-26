using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Batches;
using RoastingManager.Api.Contracts.Catalogs;

namespace RoastingManager.Api.Tests.Batches;

// RF-07 a RF-13, RF-19, RF-20, RF-22: registrar y consultar batches de tueste.
public class RoastBatchesTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public RoastBatchesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => ApiFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<int> CreateGreenCoffeeAsync(decimal pricePerKg = 10m, decimal stockKg = 100m)
    {
        var supplierResponse = await _client.PostAsJsonAsync(
            "/api/coffee-suppliers", new CreateCoffeeSupplierRequest("Finca La Esperanza"));
        var supplier = await supplierResponse.Content.ReadFromJsonAsync<CoffeeSupplierResponse>();

        var greenCoffeeResponse = await _client.PostAsJsonAsync(
            "/api/green-coffees",
            new CreateGreenCoffeeRequest("Colombia Huila", supplier!.Id, pricePerKg, stockKg));
        var greenCoffee = await greenCoffeeResponse.Content.ReadFromJsonAsync<GreenCoffeeResponse>();
        return greenCoffee!.Id;
    }

    [Fact]
    public async Task AC07a_RegistrarBatch_ConDatosValidos_GuardaElRegistro()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(pricePerKg: 10m, stockKg: 100m);

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), 11.5m, 0.3m, 15m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RoastBatchResponse>();
        Assert.Equal(10m, body!.GreenCoffeeUsedKg);
        Assert.Equal(8m, body.RoastedCoffeeObtainedKg);
        Assert.Equal(20.00m, body.ShrinkagePercentage);
        Assert.Equal(12.50m, body.CostPerKg);
        Assert.Equal(10m, body.PurchasePricePerKgAtBatchTime);
    }

    [Theory]
    [InlineData(0, 8)]
    [InlineData(10, 0)]
    [InlineData(-1, 8)]
    public async Task AC07b_RegistrarBatch_ConCantidadesInvalidas_RechazaLaOperacion(decimal used, decimal obtained)
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, used, obtained, new DateOnly(2026, 10, 20), null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("cantidades deben ser mayores a cero", body);
    }

    [Fact]
    public async Task AC08_RegistrarBatch_DescuentaStockDeCafeVerde()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(stockKg: 100m);

        await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));

        var greenCoffees = await _client.GetFromJsonAsync<List<GreenCoffeeResponse>>("/api/green-coffees");
        var updated = greenCoffees!.Single(c => c.Id == greenCoffeeId);
        Assert.Equal(90m, updated.StockKg);
    }

    [Fact]
    public async Task AC09_RegistrarBatch_IncrementaStockDeCafeTostado()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync();

        await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));

        var stock = await _client.GetFromJsonAsync<RoastedCoffeeStockResponse>("/api/roasted-coffee-stock");
        Assert.Equal(8m, stock!.QuantityKg);
    }

    [Fact]
    public async Task AC10_RegistrarBatch_SinStockSuficienteDeCafeVerde_RechazaLaOperacion()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(stockKg: 5m);

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("no existe stock suficiente", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AC11_RegistrarBatch_CalculaElPorcentajeDeMerma()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8.5m, new DateOnly(2026, 10, 20), null, null, null));

        var body = await response.Content.ReadFromJsonAsync<RoastBatchResponse>();
        Assert.Equal(15.00m, body!.ShrinkagePercentage);
    }

    [Fact]
    public async Task AC12_RegistrarBatch_FijaElPrecioDeCompraVigenteDeFormaPermanente()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(pricePerKg: 10m, stockKg: 100m);

        var batchResponse = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));
        var batch = await batchResponse.Content.ReadFromJsonAsync<RoastBatchResponse>();

        Assert.Equal(10m, batch!.PurchasePricePerKgAtBatchTime);
        Assert.Equal(12.50m, batch.CostPerKg);

        // El precio/costo fijados deben persistir igual al consultar el historial.
        var history = await _client.GetFromJsonAsync<List<RoastBatchResponse>>("/api/roast-batches");
        var persisted = history!.Single(b => b.Id == batch.Id);
        Assert.Equal(10m, persisted.PurchasePricePerKgAtBatchTime);
        Assert.Equal(12.50m, persisted.CostPerKg);
    }

    [Fact]
    public async Task AC13_RegistrarDosBatches_ActualizaElCostoPromedioPonderado()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(pricePerKg: 10m, stockKg: 100m);

        // Batch 1: 10kg verde -> 8kg tostado a costo (10*10/8) = 12.50/kg.
        await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));

        // Batch 2: 10kg verde -> 9kg tostado a costo (10*10/9) = 11.11/kg.
        await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 9m, new DateOnly(2026, 10, 21), null, null, null));

        var stock = await _client.GetFromJsonAsync<RoastedCoffeeStockResponse>("/api/roasted-coffee-stock");

        // ((8*12.50)+(9*11.11))/(8+9) = 199.99/17 = 11.76
        Assert.Equal(17m, stock!.QuantityKg);
        Assert.Equal(11.76m, stock.WeightedAverageCostPerKg);
    }

    [Fact]
    public async Task AC19_ConsultarStockDisponible_MuestraElStockDeCafeVerdeYTostado()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(stockKg: 100m);

        await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));

        var greenCoffees = await _client.GetFromJsonAsync<List<GreenCoffeeResponse>>("/api/green-coffees");
        Assert.Equal(90m, greenCoffees!.Single(c => c.Id == greenCoffeeId).StockKg);

        var roastedStock = await _client.GetFromJsonAsync<RoastedCoffeeStockResponse>("/api/roasted-coffee-stock");
        Assert.Equal(8m, roastedStock!.QuantityKg);
    }

    [Fact]
    public async Task AC20_ConsultarHistorialDeBatches_MuestraLosBatchesRegistrados()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync();

        await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));

        var history = await _client.GetFromJsonAsync<List<RoastBatchResponse>>("/api/roast-batches");

        Assert.Single(history!);
        Assert.Equal("Colombia Huila", history![0].GreenCoffeeName);
    }

    [Fact]
    public async Task AC22_ConsultarCostoDeProduccionPorBatch_MuestraElCostoCalculado()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null));
        var created = await response.Content.ReadFromJsonAsync<RoastBatchResponse>();

        var history = await _client.GetFromJsonAsync<List<RoastBatchResponse>>("/api/roast-batches");
        var persisted = history!.Single(b => b.Id == created!.Id);

        Assert.Equal(12.50m, persisted.CostPerKg);
    }

    // --- Regresión: merma negativa (RoastedCoffeeObtainedKg > GreenCoffeeUsedKg) ---
    // Reportado: un tueste de 1kg verde que "dio" 5kg tostado entraba con
    // merma de -400%, y ese costo falso contaminaba el costo promedio
    // ponderado del pool (y con él, el margen de las ventas siguientes).

    [Fact]
    public async Task RechazaBatch_CuandoElTostadoObtenidoSuperaAlVerdeUtilizado()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(stockKg: 10m);

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 1m, 5m, new DateOnly(2026, 10, 20), null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("no puede ser mayor a la cantidad de café verde utilizada", body);
    }

    [Fact]
    public async Task RechazaBatchConMermaNegativa_SinTocarElStockNiElPool()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(stockKg: 10m);

        await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 1m, 5m, new DateOnly(2026, 10, 20), null, null, null));

        var greenCoffees = await _client.GetFromJsonAsync<List<GreenCoffeeResponse>>("/api/green-coffees");
        Assert.Equal(10m, greenCoffees!.Single(c => c.Id == greenCoffeeId).StockKg);

        var stock = await _client.GetFromJsonAsync<RoastedCoffeeStockResponse>("/api/roasted-coffee-stock");
        Assert.Equal(0m, stock!.QuantityKg);
        Assert.Equal(0m, stock.WeightedAverageCostPerKg);
    }

    [Fact]
    public async Task PermiteMermaCero_CuandoElTostadoObtenidoIgualaAlVerdeUtilizado()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(stockKg: 10m);

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 5m, 5m, new DateOnly(2026, 10, 20), null, null, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RoastBatchResponse>();
        Assert.Equal(0m, body!.ShrinkagePercentage);
    }

    // --- Regresión: condición de carrera en el descuento de stock de café verde ---
    // Reportado: con 10kg de Kenia, 5 tuestes concurrentes de 3kg entraron
    // los 5 y el stock quedó en 7kg (debería haber entrado como máximo 3 y
    // terminar en 1kg). El controller leía el stock, validaba en memoria y
    // "SaveChanges" pisaba el valor: todos los requests concurrentes leían
    // el mismo stock inicial y pasaban la validación antes de que ninguno
    // hubiera escrito todavía (lost update).

    [Fact]
    public async Task BatchesConcurrentes_NoPermitenSobreventaDeStockDeCafeVerde()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(stockKg: 10m);

        var requests = Enumerable.Range(0, 5).Select(_ => _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 3m, 2m, new DateOnly(2026, 10, 20), null, null, null)));

        var responses = await Task.WhenAll(requests);

        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var rejectedCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        // Con 10kg de stock y lotes de 3kg, a lo sumo entran 3 (9kg); nunca los 5.
        Assert.Equal(3, successCount);
        Assert.Equal(2, rejectedCount);

        var greenCoffees = await _client.GetFromJsonAsync<List<GreenCoffeeResponse>>("/api/green-coffees");
        var updated = greenCoffees!.Single(c => c.Id == greenCoffeeId);

        Assert.Equal(1m, updated.StockKg); // 10 - 3*3 = 1, nunca negativo ni "pisado" en 7
        Assert.True(updated.StockKg >= 0);
    }

    [Fact]
    public async Task BatchesConcurrentes_ActualizanCorrectamenteElPoolDeTostadoSinPerderIncrementos()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync(pricePerKg: 10m, stockKg: 30m);

        // 3 batches idénticos (10kg -> 8kg a $12.50/kg): mismo costo en los
        // tres, así el resultado final no depende del orden de ejecución.
        var requests = Enumerable.Range(0, 3).Select(_ => _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2026, 10, 20), null, null, null)));

        var responses = await Task.WhenAll(requests);

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));

        var stock = await _client.GetFromJsonAsync<RoastedCoffeeStockResponse>("/api/roasted-coffee-stock");

        Assert.Equal(24m, stock!.QuantityKg); // 3 * 8kg, sin perder ningún incremento
        Assert.Equal(12.50m, stock.WeightedAverageCostPerKg);
    }

    // --- Regresión: fechas vacías/futuras ---
    // Reportado: una fecha vacía quedaba guardada como 0001-01-01 en vez de
    // rechazarse, y el sistema aceptaba fechas como 2099.

    [Fact]
    public async Task RechazaBatch_ConFechaFaltante()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, null, null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("fecha del tueste es obligatoria", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RechazaBatch_ConFechaFutura()
    {
        var greenCoffeeId = await CreateGreenCoffeeAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/roast-batches",
            new CreateRoastBatchRequest(greenCoffeeId, 10m, 8m, new DateOnly(2099, 1, 1), null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("no puede ser futura", body, StringComparison.OrdinalIgnoreCase);
    }
}
