using FingridDatahubLogger.Services;
using FingridDatahubLogger.Services.DatahubModels;
using Microsoft.AspNetCore.Mvc;

namespace FingridDatahubLogger.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;
    private readonly IDatahubClient _datahubClient;
    private readonly ITimescaleClient _timescaleClient;

    public ApiController(ILogger<ApiController> logger, IDatahubClient datahubClient, ITimescaleClient timescaleClient)
    {
        _logger = logger;
        _datahubClient = datahubClient;
        _timescaleClient = timescaleClient;
    }
    
    [HttpPost("GetConsumptions")]
    public Task<TimeSeriesResponse> GetConsumptions([FromBody] ConsumptionRequest request)
    {
        return _datahubClient.GetConsumptionsAsync(request);
    }
    
    [HttpPost("GetOrganisations")]
    public Task<OrganisationData[]> GetOrganisations()
    {
        return _datahubClient.GetOrganisationsAsync();
    }
    
    [HttpPost("UpdateConsumptions")]
    public async Task<TimeSeriesResponse> UpdateConsumptions([FromBody] ConsumptionRequest request, CancellationToken cancellationToken)
    {
        var consumptions = await _datahubClient.GetConsumptionsAsync(request);
        
        _logger.LogInformation("Received {TimeSeriesCount} where first timeseries has {ObservationCount} observations", 
            consumptions.TimeSeries.Count, consumptions.TimeSeries[0].Observations.Count);
        
        // The response ResolutionDuration is weird because it doesn't match the actual ResolutionDuration so let's fix it
        consumptions.TimeSeries.ForEach(ts => ts.ResolutionDuration = request.ResolutionDuration);
        
        // Save to database
        await _timescaleClient.InsertConsumptionsAsync(consumptions, cancellationToken);
        
        return consumptions;
    }
}