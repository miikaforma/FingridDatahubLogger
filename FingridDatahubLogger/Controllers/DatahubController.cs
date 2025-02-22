using FingridDatahubLogger.Services;
using FingridDatahubLogger.Services.DatahubModels;
using Microsoft.AspNetCore.Mvc;

namespace FingridDatahubLogger.Controllers;

[ApiController]
[Route("[controller]")]
public class DatahubController : ControllerBase
{
    private readonly ILogger<DatahubController> _logger;
    private readonly IDatahubClient _datahubClient;
    private readonly ITimescaleClient _timescaleClient;

    public DatahubController(ILogger<DatahubController> logger, IDatahubClient datahubClient, ITimescaleClient timescaleClient)
    {
        _logger = logger;
        _datahubClient = datahubClient;
        _timescaleClient = timescaleClient;
    }
    
    [HttpPost("GetAgreements")]
    public Task<AgreementData[]> GetAgreements([FromBody] AgreementRequest request)
    {
        return _datahubClient.GetAgreementDataAsync(request);
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
        try
        {
            var consumptions = await _datahubClient.GetConsumptionsAsync(request);
            
            _logger.LogInformation("Received {TimeSeriesCount} timeseries", 
                consumptions.TimeSeries.Count);

            for (var i = 0; i < consumptions.TimeSeries.Count; i++)
            {
                var timeSeries = consumptions.TimeSeries[i];
                
                _logger.LogInformation("Timeseries {Index} has {ObservationCount} observations", 
                    i, timeSeries.Observations.Count);
            }
            
            // The response ResolutionDuration is weird because it doesn't match the actual ResolutionDuration
            // when it's specified in the request so let's fix it
            if (request.ResolutionDuration != ResolutionDuration.None)
            {
                consumptions.TimeSeries.ForEach(ts => ts.ResolutionDuration = request.ResolutionDuration ?? ResolutionDuration.None);
            }
        
            // Save to database
            await _timescaleClient.InsertConsumptionsAsync(consumptions, cancellationToken);
        
            return consumptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumptions");
            throw;
        }
    }
}