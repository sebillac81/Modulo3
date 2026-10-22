using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoastingManager.Api.Data;

namespace RoastingManager.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly RoastingManagerDbContext _dbContext;

    public HealthController(RoastingManagerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();
        return Ok(new { status = "ok", database = canConnect ? "connected" : "unreachable" });
    }
}
