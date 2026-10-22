using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Contracts.Catalogs;
using RoastingManager.Api.Data;
using RoastingManager.Api.Models;

namespace RoastingManager.Api.Controllers.Catalogs;

// RF-06 / AC-06: registrar y consultar clientes.
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public CustomersController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetAll()
    {
        var customers = await _dbContext.Customers
            .OrderBy(c => c.Name)
            .Select(c => new CustomerResponse(c.Id, c.Name))
            .ToListAsync();

        return Ok(customers);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request)
    {
        var customer = new Customer { Name = request.Name };
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var response = new CustomerResponse(customer.Id, customer.Name);
        return CreatedAtAction(nameof(GetAll), response);
    }
}
