using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Infrastructure.Persistence;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public HealthController(ApplicationDbContext db) => _db = db;

    [HttpGet("db")]
    public async Task<IActionResult> CheckDb()
    {
        var canConnect = await _db.Database.CanConnectAsync();
        return Ok(new { canConnect });
    }
}
