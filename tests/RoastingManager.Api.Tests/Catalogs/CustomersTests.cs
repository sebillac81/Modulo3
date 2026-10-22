using System.Net;
using System.Net.Http.Json;
using RoastingManager.Api.Contracts.Catalogs;

namespace RoastingManager.Api.Tests.Catalogs;

// RF-06 / AC-06: registrar y consultar clientes.
public class CustomersTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;

    public CustomersTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => ApiFactory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AC06_RegistrarCliente_GuardaElRegistro()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/customers", new CreateCustomerRequest("Kiosco Centro"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var listResponse = await _client.GetFromJsonAsync<List<CustomerResponse>>("/api/customers");
        Assert.Contains(listResponse!, c => c.Name == "Kiosco Centro");
    }
}
