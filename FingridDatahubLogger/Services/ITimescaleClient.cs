using FingridDatahubLogger.Services.DatahubModels;

namespace FingridDatahubLogger.Services;

public interface ITimescaleClient
{
    Task InsertConsumptionsAsync(TimeSeriesResponse consumptions, CancellationToken cancellationToken = default);
}