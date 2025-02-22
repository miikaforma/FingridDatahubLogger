using FingridDatahubLogger.Services;
using FingridDatahubLogger.Services.DatabaseModels;
using Microsoft.AspNetCore.Mvc;

namespace FingridDatahubLogger.Controllers;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ILogger<DatabaseController> _logger;
    private readonly ITimescaleClient _timescaleClient;

    public DatabaseController(ILogger<DatabaseController> logger, ITimescaleClient timescaleClient)
    {
        _logger = logger;
        _timescaleClient = timescaleClient;
    }
    
    [HttpPost("GetMeteringPoints")]
    public Task<List<DbMeteringPoint>> GetMeteringPoints()
    {
        return _timescaleClient.GetMeteringPointsAsync();
    }
}